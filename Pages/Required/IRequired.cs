using Newtonsoft.Json.Linq;
using System;

namespace Challenges_App.Pages.Required
{
    internal interface IRequired
    {
        enum Required
        {
            Island, Items, Others, Stats
        }

        public static IRequired? loadRequired(Required type, ChallengeSession session, JObject? json)
        {
            Type? t = Type.GetType("Challenges_App.Pages.Required." + type.ToString());
            if (t != null)
            {
                if (json == null)
                {
                    return (IRequired)Activator.CreateInstance(t, session);
                }
                else
                {
                    return (IRequired)Activator.CreateInstance(t, session, json);
                }
            }
            return null;
        }

        public Required getType();

        public JObject toJson();

        public void navigate();
    }
}
