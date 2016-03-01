using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using StateInput = System.Tuple<awkwardsimulator.GameState, awkwardsimulator.Input>;
using SMath = System.Math;

namespace awkwardsimulator
{
//    public class PlayerId {
//        private int _id;
//
//        public static PlayerId P1 = new PlayerId(1);
//        public static PlayerId P2 = new PlayerId(2);
//
//        private PlayerId(int id) {
//            _id = id;
//        }
//
//        override public String ToString() {
//            return _id.ToString ();
//        }
//    }

    public enum PlayerId { P1, P2 }

    abstract public class AI {
        public PlayerId pId;

        private ForwardModel forwardModel;

        public AI(GameState stage, PlayerId pId) {
            this.pId = pId;
            this.forwardModel = new ForwardModel(stage);
        }

        protected static Random rand = new Random(1339);

//        protected List<Platform> visitedPlatforms = new List<Platform>();

//        protected static int randGeno() {
//            return rand.Next(6);
//        }

//        protected static Input fromGeno(int genotype) {
//            bool left  = (genotype & 1) > 0;
//            bool right = (genotype & 2) > 0;
//            bool up    = (genotype & 4) > 0;
//
//            return new Input(left, right, up);
//        }

//        public static Input randInput() {
//            return fromGeno(randGeno());
//        }

        //public static Input randInput() {
        //  Input[] inputs = new Input[] {
        //    new Input(false, false, false),
        //    new Input(false, false,  true),
        //    new Input(false,  true, false),
        //    new Input(false,  true,  true),
        //    new Input( true, false, false),
        //    new Input( true, false,  true)
        //    //new Input( true,  true, false),
        //    //new Input( true,  true,  true),
        //  };

        //  return inputs[rand.Next(inputs.Count())];
        //}

//        protected GameState[] evaluatePath(GameState state, int[] path) {
//            return evaluatePath(state, path.Select(fromGeno).ToArray());
//        }
//
//        protected GameState[] evaluatePath(GameState state, Input[] path) {
//            GameState[] states = new GameState[path.Length];
//            GameState lastState = state;
//
//            for (int i = 0; i < path.Length; i++) {
//                Input input = path[i];
//
//                states[i] = next(lastState, input);//, predictedPartnerInput);
//
//                lastState = states[i];
//            }
//
//            return states;
//        }
//
//        protected List<StateInput> playerInputs() {
//            List<StateInput> inputs;
//
//            switch (pId) {
//            case PlayerId.P1: inputs = Global.AI_p1Inputs; break;
//            case PlayerId.P2: inputs = Global.AI_p2Inputs; break;
//            default: throw new Exception("Unknown player id!");
//            }
//
//            return inputs;
//        }
//
//        protected List<StateInput> partnerInputs() {
//            List<StateInput> inputs;
//
//            switch (pId) {
//            case PlayerId.P1: inputs = Global.AI_p2Inputs; break;
//            case PlayerId.P2: inputs = Global.AI_p1Inputs; break;
//            default: throw new Exception("Unknown player id!");
//            }
//
//            return inputs;
//        }

        protected Player thisPlayer(GameState state) {
            Player player;

            switch (pId) {
            case PlayerId.P1: player = state.p1; break;
            case PlayerId.P2: player = state.p2; break;
            default: throw new Exception("Unknown player id!");
            }

            return player;
        }

        protected Player otherPlayer(GameState world) {
            Player player;

            switch (pId) {
            case PlayerId.P1: player = world.p2; break;
            case PlayerId.P2: player = world.p1; break;
            default: throw new Exception("Unknown player id!");
            }

            return player;
        }

        protected GameState next(GameState game, Input move) {
//            return next(game, move, pId == PlayerId.P1 ? game.p2_previousInput : game.p1_previousInput);
            return next(game, move, move); //TODO don't just copy this-player's move
        }

        protected GameState next(GameState game, Input thisPlayerMove, Input otherPlayerMove) {
            GameState lastState = game;

            int intermediateSteps = 11; // set to match a reasonable Gameloop#humanInputDelayFrames() value...
            for (int i = 0; i < intermediateSteps; i++) {
                if (pId == PlayerId.P1) {
                    lastState = forwardModel.next(lastState, thisPlayerMove, otherPlayerMove);
                } else {
                    lastState = forwardModel.next(lastState, otherPlayerMove, thisPlayerMove);
                }
            }

            return lastState;
        }

//        protected Platform myPlatform(GameState game) {
//            Player me = pId == PlayerId.P1 ? game.p1 : game.p2;
//            return myPlatform(game, me);
//        }

//        public static PlatformAIData myPlatform(GameState game, PlayerAIData me) {
//            StageAIData stage = game.stage;
//            List<PlatformAIData> platforms = stage.GetPlatforms();
//
//            PlatformAIData activePlatform = null;   
//            foreach (PlatformAIData platform in platforms) {
//                if (platform.GetYPosition() > me.GetYPosition()) {    //This is ">" since the Y coordinates are inverted
//                    if (me.GetXPosition() >= platform.GetXPosition() - platform.GetWidth()/2 && me.GetXPosition() <= platform.GetXPosition() + platform.GetWidth()/2) {
//                        if (activePlatform == null || activePlatform.GetYPosition() > platform.GetYPosition()) {
//                            activePlatform = platform;
//                        }
//                    }
//                }
//            }
//            return activePlatform;
//        }


//        protected float closestPlatform(GameState game, PlatformAIData myPlatform) {
//            float buffer = 1;       //Buffer is the forgiveness in determining the edge of the platforms
//            PlayerAIData me = pId == PlayerId.P1 ? game.p1 : game.p2;
//
//            StageAIData stage = game.stage;
//            List<PlatformAIData> platforms = stage.GetPlatforms();
//
//            float minDistance = 100;
//            foreach (PlatformAIData platform in platforms) {
//                if (!visitedPlatforms.Contains(platform)) {
//                    float xLeft = platform.GetXPosition() - platform.GetWidth()/2 + 10;
//                    float xRight = platform.GetXPosition() + platform.GetWidth()/2;
//                    float y = platform.GetYPosition() + 5 + buffer;
//
//                    minDistance = System.Math.Min(minDistance, Vec2.Distance(new Vec2(xLeft, y), me.GetPosition()));
//                    minDistance = System.Math.Min(minDistance, Vec2.Distance(new Vec2(xRight, y), me.GetPosition()));
//                }
//            }
//            return minDistance;
//        }

//        public float linearHealthHeuristic(GameState game) {
//            //float hs = .05f;                  //Health Sensitivety: between 0 and 1 exclusive
//            //int health = Math.Ceiling(1/(game.health - game.health^2 + hs) - 1/(.25 + hs));
//            //float health = 100.0f * ((float) System.Math.Pow(0.5f - game.health, 2.0f));
//
//            //System.Console.WriteLine("Health: {0}", health);
//
//            Vec2 position = (pId == PlayerId.P1) ? game.p1.GetPosition() : game.p2.GetPosition();
//            float goal = Vec2.Distance(position, game.stage.GetGoal().GetPosition());
//
//            float minHealth = 0.5f;
//            float health =  System.Math.Abs(game.health - minHealth);
//            health  = goal * health;
//
//            //Console.WriteLine(health);
//
//            return goal + health;
//        }
//
//        protected float heuristic(GameState game) {
//            //float hs = .05f;                  //Health Sensitivety: between 0 and 1 exclusive
//            //int health = Math.Ceiling(1/(game.health - game.health^2 + hs) - 1/(.25 + hs));
//            float health = 20.0f * ((float) System.Math.Pow(0.5f - game.health, 2.0f));
//            //System.Console.WriteLine("Health: {0}", health);
//
//            //Distance to goal
//            PlayerAIData me = pId == PlayerId.P1 ? game.p1 : game.p2;
//            PlatformAIData activePlatform = this.myPlatform(game);
//            if (activePlatform != null && !visitedPlatforms.Contains(activePlatform)) {
//                visitedPlatforms.Add(activePlatform);
//                //System.Console.WriteLine("platform added");
//            }
//
//            float ypos = activePlatform != null ? activePlatform.GetYPosition() : me.GetYPosition(); 
//            float xpos = me.GetXPosition();
//
//            //Distance to other platforms
//            float platformDistance = this.closestPlatform(game, activePlatform);
//
//            //Look at platform edges
//            //Rate platorms by distance to goal?
//            //Explore nearby platforms
//            //Ability to climb other platforms
//
//            //Total
//            float goal = 100*System.Math.Abs(xpos - game.stage.GetGoal().GetXPosition()) + 1000*System.Math.Abs(ypos - game.stage.GetGoal().GetYPosition());
//            goal += 100*platformDistance;
//
//            if (pId == PlayerId.P2) {
//                //System.Console.WriteLine(this.closestPlatform(game, activePlatform));
//            }
//
//            return goal + 100*health;
//        }

//        abstract public Input nextInput(GameState state);
//
//        public static int manhattanDist(PlayerAIData p1, PlayerAIData p2) {
//            return manhattanDist(new V2(p1.GetPosition()), p2);
//        }
//
//        public static int manhattanDist(GoalAIData goal, PlayerAIData player) {
//            return manhattanDist(new V2(goal.GetPosition().X, goal.GetPosition().Y), player);
//        }
//
//        public static double manhattanDistD(GoalAIData goal, PlayerAIData player) {
//            return manhattanDistD(new V2(goal.GetPosition().X, goal.GetPosition().Y), player);
//        }
//
//        public static int manhattanDist(V2 goal, PlayerAIData player) {
//            return (int) manhattanDistD(goal, player);
//        }
//
//        public static double manhattanDistD(V2 goal, PlayerAIData player) {
//            double manhattanDist = System.Math.Abs(goal.x - player.GetXPosition()) +
//                System.Math.Abs(goal.y - player.GetYPosition());
//
//            return manhattanDist;
//        }
    }

//    abstract public class SupervisedAI : AI {
//        public SupervisedAI(Stage stage, PlayerId pId) : base(stage, pId) {}
//
//        protected double direction(double p1x, double p1y, double p2x, double p2y) {
//            //return Math.Atan2(p2y, p2x) - Atan2(p1y, p1x);
//            return System.Math.Atan((p2x - p1x)/(p2y - p1y));
//        }
//
//        protected Vector<double> toBools(Input input) {
//            return new DenseVector(new [] {
//                input.left ? 1.0 : 0.0,
//                input.right ? 1.0 : 0.0,
//                input.up ? 1.0 : 0.0
//            });
//        }
//
//        protected Input toInput(Vector<double> buttonWeights) {
//            return new Input((buttonWeights[0] >= .5),
//                (buttonWeights[1] >= .5),
//                (buttonWeights[2] >= .5));
//        }
//
//        public class BoolFeature {
//            public string name;
//            public Func<GameState, bool> run;
//
//            public BoolFeature(string name, Func<GameState, bool> run) {
//                this.name = name;
//                this.run = run;
//            }
//
//            override public string ToString() {
//                return name;
//            }
//        }
//
//        public delegate double Feature(GameState state);
//
//        double getDirectionToGoal(GameState state) {
//            PlayerAIData player = thisPlayer(state);
//            GoalAIData goal = state.stage.GetGoal();
//            return direction(player.GetXPosition(), player.GetYPosition(),
//                goal.GetXPosition(), goal.GetYPosition());
//        }
//        double goalXDist(GameState state) {
//            PlayerAIData player = thisPlayer(state);
//            GoalAIData goal = state.stage.GetGoal();
//
//            return System.Math.Abs(player.GetXPosition() - goal.GetXPosition());
//        }
//        double goalYDist(GameState state) {
//            PlayerAIData player = thisPlayer(state);
//            GoalAIData goal = state.stage.GetGoal();
//            return System.Math.Abs(player.GetYPosition() - goal.GetYPosition());
//        }
//        double goalDistXYRatio(GameState state) {
//            return goalYDist(state)/goalXDist(state);
//        }
//        double partnerXDist(GameState state) { 
//            PlayerAIData player = thisPlayer(state);
//            PlayerAIData partner = otherPlayer(state);
//            return partner.GetXPosition() - player.GetXPosition();
//        }
//        double partnerYDist(GameState state) { 
//            PlayerAIData player = thisPlayer(state);
//            PlayerAIData partner = otherPlayer(state);
//            return partner.GetYPosition() - player.GetYPosition();
//        }
//
//        protected List<BoolFeature> boolFeatures() {
//            return new List<BoolFeature>() {
//                new BoolFeature("goal-direction", (s) => getDirectionToGoal(s) > 0),
//                new BoolFeature("player-grounded", (s) => thisPlayer(s).GetGrounded()),
//                //new BoolFeature("goal-distance-x-far",  (s) => goalXDist(s) > 40),
//                new BoolFeature("goal-distance-x-med",  (s) => goalXDist(s) > 20),
//                //new BoolFeature("goal-distance-x-near", (s) => goalXDist(s) > 10),
//                //new BoolFeature("goal-distance-y-far",  (s) => goalYDist(s) > 40),
//                new BoolFeature("goal-distance-y-med",  (s) => goalYDist(s) > 20),
//                //new BoolFeature("goal-distance-y-near", (s) => goalYDist(s) > 10),
//            };
//        }
//
//        protected Vector<double> features(GameState state) {
//            //double standingOnPlat;
//            //double distToPlatEdge;
//            PlayerAIData player = thisPlayer(state);
//            PlayerAIData partner = otherPlayer(state);
//            GoalAIData goal = state.stage.GetGoal();
//
//            double directionToGoal = getDirectionToGoal(state);
//            double goalDistXYRatio = (goal.GetYPosition() - player.GetYPosition())/
//                (player.GetXPosition() - goal.GetXPosition());
//            double partnerXDist = partner.GetXPosition() - player.GetXPosition();
//            double partnerYDist = partner.GetYPosition() - player.GetYPosition();;
//
//            Vector<double> f = new DenseVector(new [] {
//                1.0, // Bias
//                //standingOnPlat,
//                //distToPlatEdge,
//                directionToGoal,
//                goalDistXYRatio,
//                partnerXDist,
//                partnerYDist,
//            });
//
//            return f;
//        }
//
//    }
}
