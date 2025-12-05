using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.SingletonSystem;
using UnityEngine;
using Timberborn.Forestry;
using Timberborn.GameFactionSystem;
using Timberborn.FactionSystem;
using Timberborn.BlueprintSystem;


namespace Cordial.Mods.ForestTool.Scripts
{
    public class ForestToolSpecService : ILoadableSingleton
    {

        // access to specs
        private static SpecService _specService;

        // access to faction
        private static FactionService _factionService;

        // faction information
        private static string _factionId;

        public string FactionId => _factionId;

        public ForestToolSpecService( SpecService specService,
                                      FactionService factionService)
        {
            _specService = specService;
            _factionService = factionService;
        }

        public void Load()
        {
            if ((null == _specService)
                || (null == _factionService))
            {
                Debug.LogError("ForestTool: Missing Service");
            }
            else
            {
                _factionId = GetFactionName();

                // only call parameter init once
                ForestToolParam.ForestToolSpecService = this;
                ForestToolParam.InitConfigDefault();
            }
        }

        public ImmutableArray<string> GetAllTrees()
        {
            List<string> treeTypes = new();

            if (null != _specService)
            { 
                var treeComponents = _specService.GetSpecs<TreeComponentSpec>();

                //todo Cordial: Load a spec group
                foreach (var treeObject in treeComponents)
                {
                    treeTypes.Add(treeObject.ToString());
                }
            }
            return treeTypes.ToImmutableArray<string>();
        }
        private static string GetFactionName()
        {
            string factionId = "";
            FactionSpec _activeFaction;

            if (null != _factionService)
            {
                _activeFaction = _factionService.Current;
                factionId = _activeFaction.Id;
            }

            return factionId;
        }
        public ImmutableArray<string> GetAllForestryPlantables()
        {
            List<string> treeTypes = new();

            if (null != _specService)
            {
                var treeComponents = _specService.GetSpecs<TreeComponentSpec>();
                var bushComponents = _specService.GetSpecs<BushSpec>();

                foreach (var bushObject in bushComponents)
                {
                    treeTypes.Add(bushObject.ToString());
                }

                foreach (var treeObject in treeComponents)
                {
                    treeTypes.Add(treeObject.ToString());
                }
            }
            return treeTypes.ToImmutableArray<string>();
        }
    }
}
