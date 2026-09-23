using System.Collections.Generic;

namespace DEW.Core
{
    public static class SyntaxRegistry
    {
        public static readonly List<RegistryEntry> Entries = new List<RegistryEntry> //list of patterns and their meanings
        {
            //Box line syntax. Affects displaying messages rather than touching event/game data
            new RegistryEntry { Pattern = "$b", FriendlyName = "Box Break", 
            Description = "Ends the current dialogue box and starts a new one on click.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$e", FriendlyName = "End Conversation",
            Description = "Closes the dialogue box. Anything after this waits until the next time you talk to this NPC.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$k", FriendlyName = "Kill",
            Description = "Removes all dialogue that would have played after this point.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "#", FriendlyName = "Separator",
            Description = "Glue between commands. Invisible in actual dialogue box on its own.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "@", FriendlyName = "Farmer Name",
            Description = "Replaced with the player/farmer's name.", Kind = SyntaxKind.Inline },

            new RegistryEntry { Pattern = "^", FriendlyName = "Gender Split",
            Description = "Text before this applies to male players, text after applies to female players.", Kind = SyntaxKind.Splitter },
            
            new RegistryEntry { Pattern = "$c", FriendlyName = "Chance Split",
            Description = "The number is the probability (0-1) that the first line of text is shown instead of the second.", Kind = SyntaxKind.Structural},

            new RegistryEntry { Pattern = "%", FriendlyName = "Narration Box",
            Description = "Only meaningful at the very start of the line. Displays plain text with no portrait.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$action", FriendlyName = "Action Handler",
            Description = "Runs a trigger action string. Must be placed at the end of dialogue and can only be chained with other $action when separated by #.",
            Kind = SyntaxKind.Structural},

            new RegistryEntry { Pattern = "%revealtaste", FriendlyName = "Reveal Item Preference",
            Description = "Reveals an NPC's gift preference for an item.", Kind = SyntaxKind.Structural},

            new RegistryEntry { Pattern = "$t", FriendlyName = "Topic Setter",
            Description = "Adds a conversation topic for the next [X] days.", Kind = SyntaxKind.Structural},

            new RegistryEntry { Pattern = "$v", FriendlyName = "Event Handler",
            Description = "Immediately start the <event ID> event and end current dialogue. Need to set true or false to check event preconditions as well as true or false to skip the event if seen (default: true)",
            Kind = SyntaxKind.Structural},



            //Portrait tags
            new RegistryEntry { Pattern = "$0", FriendlyName = "Portrait: Neutral",
            Description = "Shows the neutral portrait for this box.", Kind = SyntaxKind.Portrait },

            new RegistryEntry { Pattern = "$1", FriendlyName = "Portrait: Happy",
            Description = "Shows the happy portrait for this box.", Kind = SyntaxKind.Portrait },

            new RegistryEntry { Pattern = "$h", FriendlyName = "Portrait: Happy",
            Description = "Shows the happy portrait for this box.", Kind = SyntaxKind.Portrait },

            new RegistryEntry { Pattern = "$2", FriendlyName = "Portrait: Sad",
            Description = "Shows the sad portrait for this box.", Kind = SyntaxKind.Portrait },

            new RegistryEntry { Pattern = "$s", FriendlyName = "Portrait: Sad",
            Description = "Shows the sad portrait for this box.", Kind = SyntaxKind.Portrait },

            new RegistryEntry { Pattern = "$3", FriendlyName = "Portrait: Unique",
            Description = "Shows this NPC's unique portrait for this box.", Kind = SyntaxKind.Portrait },

            new RegistryEntry { Pattern = "$u", FriendlyName = "Portrait: Unique",
            Description = "Shows this NPC's unique portrait for this box.", Kind = SyntaxKind.Portrait },

            new RegistryEntry { Pattern = "$4", FriendlyName = "Portrait: Love",
            Description = "Shows the love portrait for this box.", Kind = SyntaxKind.Portrait },

            new RegistryEntry { Pattern = "$l", FriendlyName = "Portrait: Love",
            Description = "Shows the love portrait for this box.", Kind = SyntaxKind.Portrait },

            new RegistryEntry { Pattern = "$5", FriendlyName = "Portrait: Angry",
            Description = "Shows the angry portrait for this box.", Kind = SyntaxKind.Portrait },

            new RegistryEntry { Pattern = "$a", FriendlyName = "Portrait: Angry",
            Description = "Shows the angry portrait for this box.", Kind = SyntaxKind.Portrait },

            new RegistryEntry { Pattern = "$6", FriendlyName = "Portrait: Custom",
            Description = "Shows a custom, mod- or NPC-specific portrait for this box.", Kind = SyntaxKind.Portrait },



            //known %word tokens
            new RegistryEntry { Pattern = "%pet", FriendlyName = "Pet Name",
            Description = "Replaced with the player's pet's name.", Kind = SyntaxKind.Inline },

            new RegistryEntry { Pattern = "%farm", FriendlyName = "Farm Name",
            Description = "Replaced with the player's farm name.", Kind = SyntaxKind.Inline },
            
            new RegistryEntry { Pattern = "%spouse", FriendlyName = "Spouse Name",
            Description = "Replaced with the player's spouse's name.", Kind = SyntaxKind.Inline },

            new RegistryEntry { Pattern = "%kid1", FriendlyName = "Player's First Child's Name",
            Description = "Replaced with the player's first child's name", Kind = SyntaxKind.Inline },

            new RegistryEntry { Pattern = "%kid2", FriendlyName = "Player's Second Child's Name",
            Description = "Replaced with the player's second child's name", Kind = SyntaxKind.Inline },

            new RegistryEntry { Pattern = "%favorite", FriendlyName = "Favorite Thing",
            Description = "Replaced with the player's favorite thing.", Kind = SyntaxKind.Inline },
        };

        public static RegistryEntry? Lookup(string rawText) // method to lookup syntax using SyntaxRegistry
        {
            foreach (var entry in Entries)
            {
                if (entry.Pattern == rawText)
                    return entry;
            }

            return null;
        }

        public static RegistryEntry? Resolve(string rawText) //exact match first, then command-name fallback
        {
            var exact = Lookup(rawText);
            if (exact != null) return exact; //try look up exact match and if successful, return it

            if (!(rawText.StartsWith("$") || rawText.StartsWith("%"))) return null; //check to see if it starts with $

            int j = 1;
            while (j < rawText.Length && char.IsLetter(rawText[j])) j++; //walk through command letter
            if (j == 1) return null; //checks to see if there are letters after $

            return Lookup(rawText.Substring(0, j)); //e.g. $action AddMoney 500  -> "$action"
        }
    }
}