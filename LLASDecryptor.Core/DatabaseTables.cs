using System.Collections.Generic;

namespace LLASDecryptor.Core
{
    public static class DatabaseTables
    {
        public static List<Table> Tables { get; } = new List<Table>()
        {
            new EncryptedTable("adv_script", "Adv Script"),
            new EncryptedTable("background", "Background"),
            new EncryptedTable("gacha_performance", "Gacha Performance"),
            new EncryptedTable("live2d_sd_model", "Live2D SD Model"),
            new EncryptedTable("live_prop_skeleton", "Live Prop Skeleton"),
            new EncryptedTable("live_timeline", "Live Timeline"),
            new AudioTable("m_asset_sound", "Sound Assets"),
            new EncryptedTable("member_model", "Member Model (3D)"),
            new EncryptedTable("member_sd_model", "Member SD Model (2D)"),
            new EncryptedTable("navi_motion", "Navi Motion"),
            new EncryptedTable("navi_timeline", "Navi Timeline"),
            new EncryptedTable("shader", "Shader"),
            new EncryptedTable("skill_effect", "Skill Effect"),
            new EncryptedTable("skill_timeline", "Skill Timeline"),
            new EncryptedTable("skill_wipe", "Skill Wipe"),
            new EncryptedTable("stage", "Stage"),
            new EncryptedTable("stage_effect", "Stage Effect"),
            new EncryptedTable("texture", "Texture (e.g. Cards)")
        };
    }
}
