using System.Collections.Generic;

namespace DEW.Core
{
    public static class SyntaxRegistry
    {
        public static readonly List<RegistryEntry> Entries = new List<RegistryEntry> //list of patterns and their meanings
        {
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
            
            new RegistryEntry { Pattern = "%", FriendlyName = "Narration Box",
            Description = "Only meaningful at the very start of the line. Displays plain text with no portrait.", Kind = SyntaxKind.LineLevel },



            //Portrait tags
            new RegistryEntry { Pattern = "$0", FriendlyName = "Portrait: Neutral",
            Description = "Shows the neutral portrait for this box.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$1", FriendlyName = "Portrait: Happy",
            Description = "Shows the happy portrait for this box.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$h", FriendlyName = "Portrait: Happy",
            Description = "Shows the happy portrait for this box.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$2", FriendlyName = "Portrait: Sad",
            Description = "Shows the sad portrait for this box.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$s", FriendlyName = "Portrait: Sad",
            Description = "Shows the s portrait for this box.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$3", FriendlyName = "Portrait: Unique",
            Description = "Shows this NPC's unique portrait for this box.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$u", FriendlyName = "Portrait: Unique",
            Description = "Shows this NPC's unique portrait for this box.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$4", FriendlyName = "Portrait: Love",
            Description = "Shows the love portrait for this box.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$l", FriendlyName = "Portrait: Love",
            Description = "Shows the love portrait for this box.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$5", FriendlyName = "Portrait: Angry",
            Description = "Shows the angry portrait for this box.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$a", FriendlyName = "Portrait: Angry",
            Description = "Shows the angry portrait for this box.", Kind = SyntaxKind.LineLevel },

            new RegistryEntry { Pattern = "$6", FriendlyName = "Portrait: Custom",
            Description = "Shows a custom, mod- or NPC-specific portrait for this box.", Kind = SyntaxKind.LineLevel },



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
    }
}