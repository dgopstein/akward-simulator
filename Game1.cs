#region Using Statements
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Linq;

#endregion

namespace awkwardsimulator
{
	// This is the main type for your game.
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		SpriteFont spriteFont;
		GameState state;
		ForwardModel forwardModel;
		Drawing drawing;
        List<GameState> history;
        PlatformAStar pas;

        public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";	            
//			graphics.IsFullScreen = true;
		}

		// load any non-graphic related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
        AI ai1, ai2;
        InputMethod inputMethod;
        ListAiInput listInputMethod;
        CombinedAiInput combinedInputMethod;
        HumanInput humanInputMethod;

        protected void Initialize (GameState level)
        {
            state = level;

            forwardModel = new ForwardModel (state);

            history = new List<GameState> ();


//            ai1 = new NullAI (state, PlayerId.P1);
            ai1 = new AStar (state, PlayerId.P1, new WaypointHeuristic (state, PlayerId.P1));
            ai2 = new AStar (state, PlayerId.P2, new WaypointHeuristic (state, PlayerId.P2));


//            listInputMethod = new ListAiInput (ai1, ai2, state);
//            inputMethod = new SynchronizedAiInput (ai1, ai2, state);
//            inputMethod = new HalfHumanAiInput (ai2, state);
            combinedInputMethod = new CombinedAiInput (state);
            humanInputMethod = new HumanInput (state, combinedInputMethod);

//            inputMethod = humanInputMethod;
            inputMethod = combinedInputMethod;
        }

        protected override void Initialize () {
            Initialize(Level.Level1);

			base.Initialize ();
		}

		// called once per game and is the place to load all of your content.
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);
			spriteFont = Content.Load<SpriteFont>("Default");

            drawing = new Drawing (GraphicsDevice, spriteBatch, spriteFont,
                Content, forwardModel.World, new Vector2(GameState.Width, GameState.Height));
		}

		protected override void UnloadContent()
		{
			base.UnloadContent();
			spriteBatch.Dispose();
			drawing.DisposeTextures();
		}

        bool enableSpacePausing = true;
        bool spaceHeld = false;
        private bool isSpaceTapped(KeyboardState keyState) {
            bool ret  = keyState.IsKeyDown (Keys.Space) && !spaceHeld;
            spaceHeld = keyState.IsKeyDown (Keys.Space);

            return ret || !enableSpacePausing;
        }

		// checking for collisions, gathering input, and playing audio.
		protected override void Update (GameTime gameTime)
		{
            KeyboardState keyState = Keyboard.GetState ();

            if (keyState.IsKeyDown (Keys.Escape)) {
                Exit ();
            } else if (keyState.IsKeyDown (Keys.D1)) {
                Initialize (Level.Level1);
            } else if (keyState.IsKeyDown (Keys.D2)) {
                Initialize (Level.Level2);
            } else if (isSpaceTapped(keyState)) {
                Tuple<Input, Input> inputs = inputMethod.Inputs ();

//                Debug.WriteLine ("{0}", inputs.Item2);
                state = forwardModel.nextState (state, inputs.Item1, inputs.Item2);

                inputMethod.Update (state); // Always update AiInput because it calculates pretty pictures

                history.Add (state);
            }

            base.Update (gameTime);
		}

		/// This is called when the game should draw itself.
		protected override void Draw (GameTime gameTime)
		{
            Player p1 = ai1.thisPlayer (state);
            Player p2 = ai2.thisPlayer (state);

			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);
		
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            drawing.DrawFPS (gameTime);

            foreach (var plat in state.Platforms) {
                drawing.DrawPlatform (plat, Color.Gainsboro);
            }

            drawing.DrawGameObjectCircle (state.Goal, Color.BurlyWood);

            drawing.DrawHealth (state.Health);

            drawing.DrawPlayStatus (state.PlayStatus);
            drawing.DrawPos (state.P1, 20, 110);
            drawing.DrawPos (state.P2, 20, 140);

            // Draw histories
            // drawing.DrawPath (history.Select (s => s.P1.Coords), Color.Thistle, 2);
            drawing.DrawPath (history.Select (s => s.P2.Coords + (Player.Size *.5f)), Color.Thistle, 2);

            // Jump height
            drawing.DrawLine(
                new Vector2(p2.LeftBoundary , p2.TopBoundary+PlatformUtil.remainingJumpDist(p2)),
                new Vector2(p2.RightBoundary, p2.TopBoundary+PlatformUtil.remainingJumpDist(p2)),
                Color.Fuchsia, 6
            );

            drawing.DrawPlayer (state.P1);
            drawing.DrawPlayer (state.P2);

            drawing.DrawMethodVisualizations (state, combinedInputMethod);
            // drawing.DrawMethodVisualizations (state, listInputMethod);

            // Draw Input movement direction arrows
            var c1 = Color.Black;
            var c2 = Color.Black;
            drawing.DrawButtonArrow (state.P1, inputMethod.input1, c1);
            drawing.DrawButtonArrow (state.P2, inputMethod.input2, c2);

            // Draw Rainbow strip at the bottom of the screen
            int n = 14;
            drawing.DrawPaths (
                Enumerable.Range(0, n).Select(x => Tuple.Create<double, IEnumerable<Vector2>>(1 - x/(float)n,
                    new List<Vector2>() { new Vector2((x+1)*10, 5), new Vector2((x+2)*10, 5)})
                )
            , 4);

			spriteBatch.End();
            			
//            drawing.DrawDebug ();

			base.Draw (gameTime);
		}
    }   
}
