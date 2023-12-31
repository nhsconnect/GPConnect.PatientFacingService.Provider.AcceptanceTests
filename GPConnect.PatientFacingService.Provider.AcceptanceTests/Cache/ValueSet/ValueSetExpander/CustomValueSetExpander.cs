﻿namespace GPConnect.PatientFacingService.Provider.AcceptanceTests.Cache.ValueSet.ValueSetExpander
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Specification.Source;
    using Hl7.Fhir.Specification.Terminology;
    using Hl7.Fhir.Utility;

    public class CustomValueSetExpander : ValueSetExpander
    {
        public CustomValueSetExpander(ValueSetExpanderSettings settings) : base(settings)
        {
        }

        public new void Expand(ValueSet source)
        {
            // Note we are expanding the valueset in-place, so it's up to the caller to decide whether
            // to clone the valueset, depending on store and performance requirements.

            source.Expansion = ValueSet.ExpansionComponent.Create();

            try
            {
                handleCompose(source);
            }
            catch (Exception)
            {
                // Expansion failed - remove (partial) expansion
                source.Expansion = null;
                throw;
            }
        }

        private void handleCompose(ValueSet source)
        {
            if (source.Compose == null) return;

            // handleImport(source);
            handleInclude(source);
            handleExclude(source);
        }


        private List<ValueSet.ContainsComponent> collectConcepts(ValueSet.ConceptSetComponent conceptSet)
        {
            List<ValueSet.ContainsComponent> result = new List<ValueSet.ContainsComponent>();

            if (!conceptSet.ValueSet.Any() && conceptSet.System == null)
                throw Error.InvalidOperation($"Encountered a ConceptSet with neither a 'system' nor a 'valueset'");

            if (conceptSet.System != null)
            {
                //if (conceptSet.Filter.Any())
                //    throw new ValueSetExpansionTooComplexException($"ConceptSets with a filter are not yet supported.");

                if (conceptSet.Concept.Any())
                {
                    foreach (var concept in conceptSet.Concept)
                    {
                        // We'd probably really have to look this code up in the original ValueSet (by system) to know something about 'abstract'
                        // and what would we do with a hierarchy if we encountered that in the include?
                        result.Add(conceptSet.System, conceptSet.Version, concept.Code, concept.Display);
                    }
                }
                else
                {
                    // Do a full import of the codesystem
                    var importedConcepts = getConceptsFromCodeSystem(conceptSet.System);

                    // Filter the full import
                    if (conceptSet.Filter.Any())
                    {
                        importedConcepts = filter(importedConcepts, conceptSet.Filter);
                    }

                    import(result, importedConcepts, conceptSet.System);
                }
            }

            if (conceptSet.ValueSet.Any())
            {
                if (conceptSet.ValueSet.Count() > 1)
                    throw new ValueSetExpansionTooComplexException($"ConceptSets with multiple valuesets are not yet supported.");
                if (conceptSet.System != null)
                    throw new ValueSetExpansionTooComplexException($"ConceptSets with combined 'system' and 'valueset'(s) are not yet supported.");

                var importedVs = conceptSet.ValueSet.Single();
                var concepts = getExpansionForValueSet(importedVs);
                import(result, concepts, importedVs);
            }

            return result;
        }

        private List<ValueSet.ContainsComponent> filter(List<ValueSet.ContainsComponent> importedConcepts, List<ValueSet.FilterComponent> filters)
        {
            var concepts = new List<ValueSet.ContainsComponent>();

            filters.ForEach(f =>
            {
                var factory = new ConceptFilterFactory(f, true);

                concepts.AddRange(factory.ApplyFilter(importedConcepts));
            });

            return concepts;
        }

        private void import(List<ValueSet.ContainsComponent> dest, List<ValueSet.ContainsComponent> source, string importeeUrl)
        {
            if (dest.Count + source.Count > Settings.MaxExpansionSize)
                throw new ValueSetExpansionTooBigException($"Import of '{importeeUrl}' ({source.Count} concepts) would be larger than the set maximum size ({Settings.MaxExpansionSize})");

            dest.AddRange(source);
        }

        private void handleInclude(ValueSet source)
        {
            if (!source.Compose.Include.Any()) return;

            int csIndex = 0;
            foreach (var include in source.Compose.Include)
            {
                var includedConcepts = collectConcepts(include);

                // Yes, exclusion could make this smaller again, but alas, before we have processed those we might have run out of memory
                if (source.Expansion.Total + includedConcepts.Count > Settings.MaxExpansionSize)
                    throw new ValueSetExpansionTooBigException($"Inclusion of {includedConcepts.Count} concepts from conceptset #{csIndex}' to  " +
                        $"valueset '{source.Url}' ({source.Expansion.Total} concepts) would be larger than the set maximum size ({Settings.MaxExpansionSize})");

                source.Expansion.Contains.AddRange(includedConcepts);

                var original = source.Expansion.Total ?? 0;
                source.Expansion.Total = original + includedConcepts.CountConcepts();
                csIndex += 1;
            }
        }

        private void handleExclude(ValueSet source)
        {
            if (!source.Compose.Exclude.Any()) return;

            foreach (var exclude in source.Compose.Exclude)
            {
                var excludedConcepts = collectConcepts(exclude);

                source.Expansion.Contains.Remove(excludedConcepts);

                var original = source.Expansion.Total ?? 0;
                source.Expansion.Total = original - excludedConcepts.CountConcepts();
            }
        }


        private List<ValueSet.ContainsComponent> getExpansionForValueSet(string uri)
        {
            if (Settings.ValueSetSource == null)
                throw Error.InvalidOperation($"No valueset resolver available to resolve valueset '{uri}', " +
                        "set ValueSetExpander.Settings.ValueSetSource to fix.");

            var importedVs = Settings.ValueSetSource.FindValueSet(uri);
            if (importedVs == null) throw new ValueSetUnknownException($"Cannot resolve canonical reference '{uri}' to ValueSet");

            if (!importedVs.HasExpansion) Expand(importedVs);

            if (importedVs.HasExpansion)
                return importedVs.Expansion.Contains;
            else
                throw new ValueSetUnknownException($"Expansion returned neither an error, nor an expansion for ValueSet with canonical reference '{uri}'");
        }

        private List<ValueSet.ContainsComponent> getConceptsFromCodeSystem(string uri)
        {
            if (Settings.ValueSetSource == null)
                throw Error.InvalidOperation($"No terminology service available to resolve references to codesystem '{uri}', " +
                        "set ValueSetExpander.Settings.ValueSetSource to fix.");

            var importedCs = Settings.ValueSetSource.FindCodeSystem(uri);
            if (importedCs == null) throw new ValueSetUnknownException($"Cannot resolve canonical reference '{uri}' to CodeSystem");

            var result = new List<ValueSet.ContainsComponent>();
            result.AddRange(importedCs.Concept.Select(c => c.ToContainsComponent(importedCs)));

            return result;
        }
    }

    public static class ContainsSetExtensions
    {
        public static ValueSet.ContainsComponent Add(this List<ValueSet.ContainsComponent> dest, string system, string version, string code, string display, IEnumerable<ValueSet.ContainsComponent> children = null)
        {
            var newContains = new ValueSet.ContainsComponent();

            newContains.System = system;
            newContains.Code = code;
            newContains.Display = display;
            newContains.Version = version;

            if (children != null)
                newContains.Contains = new List<ValueSet.ContainsComponent>(children);

            dest.Add(newContains);

            return newContains;
        }

        public static void Remove(this List<ValueSet.ContainsComponent> dest, string system, string code)
        {
            dest.RemoveAll(c => c.System == system && c.Code == code);

            // Look for this code in children too
            foreach (var component in dest)
            {
                if (component.Contains.Any())
                    component.Contains.Remove(system, code);
            }
        }

        public static void Remove(this List<ValueSet.ContainsComponent> dest, List<ValueSet.ContainsComponent> source)
        {
            //TODO: Pretty unclear what to do with child concepts in the source - they all need to be removed from dest?
            foreach (var sourceConcept in source)
                dest.Remove(sourceConcept.System, sourceConcept.Code);
        }

        internal static ValueSet.ContainsComponent ToContainsComponent(this CodeSystem.ConceptDefinitionComponent source, CodeSystem system)
        {
            var newContains = new ValueSet.ContainsComponent();

            newContains.System = system.Url;
            newContains.Version = system.Version;
            newContains.Code = source.Code;
            newContains.Display = source.Display;

            var abstractProperty = source.ListConceptProperties(system, CodeSystem.CONCEPTPROPERTY_NOT_SELECTABLE).SingleOrDefault();

            var isAbstract = (FhirBoolean)abstractProperty?.Value;
            if (isAbstract?.Value != null)
                newContains.Abstract = isAbstract.Value;

            var inactiveProperty = source.ListConceptProperties(system, CodeSystem.CONCEPTPROPERTY_STATUS).SingleOrDefault();

            var isInactive = (FhirBoolean)inactiveProperty?.Value;
            if (isInactive?.Value != null)
                newContains.Inactive = isInactive.Value;

            if (source.Concept.Any())
                newContains.Contains.AddRange(
                    source.Concept.Select(c => c.ToContainsComponent(system)));

            return newContains;
        }
    }
}
