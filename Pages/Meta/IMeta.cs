using Newtonsoft.Json.Linq;
using System;

namespace Challenges_App.Pages.Meta
{
    internal interface IMeta
    {
        enum Meta
        {
            None, Skull, Potion, LeatherArmor, Book, TropicalFish, TrimArmor
        }

        public static Meta forItem(String material)
        {
            if(material != null && material.Length > 0)
            {
                switch (material)
                {
                    case "PLAYER_HEAD": return Meta.Skull;
                    case "SPLASH_POTION":
                    case "LINGERING_POTION":
                    case "POTION": return Meta.Potion;
                    case "LEATHER_HELMET":
                    case "LEATHER_CHESTPLATE":
                    case "LEATHER_LEGGINGS":
                    case "LEATHER_BOOTS": return Meta.LeatherArmor;
                    case "WRITABLE_BOOK":
                    case "WRITTEN_BOOK": return Meta.Book;
                    case "TROPICAL_FISH_BUCKET": return Meta.TropicalFish;
                    case "TURTLE_HELMET":
                    case "CHAINMAIL_HELMET":
                    case "CHAINMAIL_CHESTPLATE":
                    case "CHAINMAIL_LEGGINGS":
                    case "CHAINMAIL_BOOTS":
                    case "IRON_HELMET":
                    case "IRON_CHESTPLATE":
                    case "IRON_LEGGINGS":
                    case "IRON_BOOTS":
                    case "DIAMOND_HELMET":
                    case "DIAMOND_CHESTPLATE":
                    case "DIAMOND_LEGGINGS":
                    case "DIAMOND_BOOTS":
                    case "GOLDEN_HELMET":
                    case "GOLDEN_CHESTPLATE":
                    case "GOLDEN_LEGGINGS":
                    case "GOLDEN_BOOTS":
                    case "NETHERITE_HELMET":
                    case "NETHERITE_CHESTPLATE":
                    case "NETHERITE_LEGGINGS":
                    case "NETHERITE_BOOTS": return Meta.TrimArmor;
                }
            }
            return Meta.None;
        }

        public static IMeta? loadMeta(Item parent, Meta meta, JObject? json)
        {
            Type? type = Type.GetType("Challenges_App.Pages.Meta." + meta.ToString());
            if (type != null)
            {
                if (json == null)
                {
                    return (IMeta)Activator.CreateInstance(type, parent);
                }
                else
                {
                    return (IMeta)Activator.CreateInstance(type, parent, json);
                }
            }
            return null;
        }

        public Meta getMeta();

        public JObject toJson();

        public void navigate();
    }
}
