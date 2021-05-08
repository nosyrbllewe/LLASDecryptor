using System.Collections.Generic;

namespace LLASDecryptor.Core
{
    public static class DatabaseTables
    {
        public static List<Table> Tables = new List<Table>()
        {
            new Table("adv_script", "Adv Script"),
            new Table("background", "Background"),
            new Table("gacha_performance", "Gacha Performance"),
            new Table("live2d_sd_model", "Live2D SD Model"),
            new Table("live_prop_skeleton", "Live Prop Skeleton"),
            new Table("live_timeline", "Live Timeline"),
            new Table("member_model", "Member Model (3D)"),
            new Table("member_sd_model", "Member SD Model (2D)"),
            new Table("navi_motion", "Navi Motion"),
            new Table("navi_timeline", "Navi Timeline"),
            new Table("shader", "Shader"),
            new Table("skill_effect", "Skill Effect"),
            new Table("skill_timeline", "Skill Timeline"),
            new Table("skill_wipe", "Skill Wipe"),
            new Table("stage", "Stage"),
            new Table("stage_effect", "Stage Effect"),
            new Table("texture", "Texture (e.g. Cards)")
        };
    }
}
