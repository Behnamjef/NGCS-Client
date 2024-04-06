using NetworkAdapter.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Tools
{
    public static class CurrentLevelCalculator
    {

        public static UserLevelViewModel? FindLevelInfoBasedOnAmount(List<LevelDTO> levels, int amount)
        {
            var model = new UserLevelViewModel();

            levels = levels.OrderBy(i => i.from_amount).ToList();
            int? previosLevelId = null;

            var UserMaxLvl = CheckIfUserMaxLevel(levels, amount);
            if (UserMaxLvl != null) return UserMaxLvl;

            foreach (var lvl in levels)
            {
                if (lvl.from_amount <= amount && amount <= lvl.to_amount)
                {
                    model.LevelTitle = lvl.title;
                    model.UserCurrentAmount = amount;
                    model.LevelTopAmount = lvl.to_amount;
                    model.LevelBottomAmount = lvl.from_amount;
                    model.LevelId = lvl.id;

                    var totalLvlAmount = Math.Abs(lvl.to_amount - lvl.from_amount);
                    var progress = Math.Abs(amount - lvl.from_amount);
                    model.LevelProgressPrc = (int)((progress / (double)totalLvlAmount) * 100);

                    if (!string.IsNullOrWhiteSpace(lvl.lvl_meta))
                    {
                        model.LevelMetaData = JsonConvert.DeserializeObject<List<LvlMeta>>(lvl.lvl_meta) ??
                        new List<LvlMeta>();
                    }

                    if (!string.IsNullOrWhiteSpace(lvl.prize_meta))
                    {
                        model.PrizeMetaData = JsonConvert.DeserializeObject<List<PrizeMetaViewModel>>(lvl.prize_meta) ??
                        new List<PrizeMetaViewModel>();
                    }

                    break;
                }

                model.PreviosLevelId = lvl.id;
            }

            return model;
        }

        private static UserLevelViewModel? CheckIfUserMaxLevel(List<LevelDTO> levels, int amount)
        {
            var model = new UserLevelViewModel();

            levels = levels.OrderBy(i => i.from_amount).ToList();
            var maxLevel = levels.LastOrDefault();

            if (amount >= maxLevel.to_amount)// check if user is max level 
            {
                model.LevelTitle = maxLevel.title;
                model.UserCurrentAmount = maxLevel.to_amount;
                model.LevelTopAmount = maxLevel.to_amount;
                model.LevelBottomAmount = maxLevel.from_amount;
                model.LevelId = maxLevel.id;

                var totalLvlAmount = Math.Abs(maxLevel.to_amount - maxLevel.from_amount);
                var progress = maxLevel.to_amount;
                model.LevelProgressPrc = 100;

                if (!string.IsNullOrWhiteSpace(maxLevel.lvl_meta))
                {
                    model.LevelMetaData = JsonConvert.DeserializeObject<List<LvlMeta>>(maxLevel.lvl_meta) ??
                new List<LvlMeta>();
                }

                if (!string.IsNullOrWhiteSpace(maxLevel.prize_meta))
                {
                    model.PrizeMetaData = JsonConvert.DeserializeObject<List<PrizeMetaViewModel>>(maxLevel.lvl_meta) ??
                new List<PrizeMetaViewModel>();
                }

                model.PreviosLevelId = levels[levels.Count - 2].id;

                return model;
            }

            return null;
        }
    }
}
