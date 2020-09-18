using System;
using System.Collections.Generic;
using System.Text;

namespace FungusBot {
    public class GameState {
        public List<Player> players;
        public VoteState voteState;

        public GameState(VoteState voteState) {
            this.voteState = voteState;
            players = new List<Player>();
        }

        public enum VoteState {
            Unknown,
            InGame,
            Voting
        }

        public class Player {
            public PlayerColor color;
            public string name;
            public bool dead;

            public Player(PlayerColor color, string name, bool dead) {
                this.color = color;
                this.name = name;
                this.dead = dead;
            }

            public enum PlayerColor {
                Red,
                Blue,
                Green,
                Pink,
                Orange,
                Yellow,
                Black,
                White,
                Purple,
                Brown,
                Aqua,
                Lime
            }
        }
    }
}
