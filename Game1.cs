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
        AiInput aiInput;
        HumanInput humanInput;
        InputMethod inputMethod;

		protected override void Initialize ()
		{
            state = Level.Level1;

            pas = new PlatformAStar (state.Platforms);

			forwardModel = new ForwardModel (state);

            history = new List<GameState> ();

            humanInput = new HumanInput (state);
            aiInput = new ListAiInput (
//            aiInput = new SingleAiInput (
                new NullAI(state, PlayerId.P1),
                new AStar (state, PlayerId.P2, new WaypointHeuristic(state, PlayerId.P2)),
                state);

//            inputMethod = humanInput;
            inputMethod = aiInput;

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

        bool spaceHeld = false;
        private bool isSpaceTapped(KeyboardState keyState) {
            bool ret  = keyState.IsKeyDown (Keys.Space) && !spaceHeld;
            spaceHeld = keyState.IsKeyDown (Keys.Space);

//            return ret; 
            return ret || true;
        }

		// checking for collisions, gathering input, and playing audio.
		protected override void Update (GameTime gameTime)
		{
            KeyboardState keyState = Keyboard.GetState ();

            if (keyState.IsKeyDown (Keys.Escape)) {
                Exit ();
            } else if (isSpaceTapped(keyState)) {
                Tuple<Input, Input> inputs = inputMethod.Inputs ();

                Debug.WriteLine ("{0}", inputs.Item2);
                state = forwardModel.nextState (state, inputs.Item1, inputs.Item2);

                aiInput.Update (state); // Always update AiInput because it calculates pretty pictures

                history.Add (state);
            }

            base.Update (gameTime);
		}
			
		/// This is called when the game should draw itself.
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);
		
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            drawing.DrawFPS (gameTime);

            foreach (var plat in state.Platforms) {
                drawing.DrawGameObjectRect (plat, Color.Gainsboro);
            }

            drawing.DrawGameObjectCircle (state.Goal, Color.BurlyWood);

            drawing.DrawPlayer (state.P1);
            drawing.DrawPlayer (state.P2);

            var c1 = Color.Black;
            var c2 = Color.Black;
            drawing.DrawButtonArrow (state.P1, inputMethod.input1, c1);
            drawing.DrawButtonArrow (state.P2, inputMethod.input2, c2);

            drawing.DrawHealth (state.Health);

            drawing.DrawPlayStatus (state.PlayStatus);
            drawing.DrawHeuristic (aiInput.ai1, state, 20, 50);
            drawing.DrawHeuristic (aiInput.ai2, state, 20, 80);
            drawing.DrawPos (state.P1, 20, 110);
            drawing.DrawPos (state.P2, 20, 140);

            drawing.DrawPath (pas.PlatformPath(state.P2, state.Goal).Select (s => s.Center), Color.Maroon, 2);
            drawing.DrawCircle (2, ((WaypointHeuristic)aiInput.ai2.Heuristic).NextWaypoint(state), Color.Crimson);

//            drawing.DrawPath (history.Select (s => s.P1.Coords), Color.Thistle, 2);
            drawing.DrawPath (history.Select (s => s.P2.Coords + (Player.Size *.5f)), Color.Thistle, 2);

            drawing.DrawPaths (aiInput.ai1.AllPaths().Select(t =>
                Tuple.Create(t.Item1, t.Item2.Select(e => e.Item2.P1.Coords))));
            drawing.DrawPaths (aiInput.ai2.AllPaths().Select(t =>
                Tuple.Create(t.Item1, t.Item2.Select(e => e.Item2.P2.Coords))));
			
			spriteBatch.End();
            			
//            drawing.DrawDebug ();

			base.Draw (gameTime);
		}
    }   
}
