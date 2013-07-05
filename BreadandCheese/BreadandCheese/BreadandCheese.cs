using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BreadandCheese
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class BreadandCheese : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //General housekeeping variables
        float displayWidth;
        float displayHeight;
        KeyboardState keys;
        SpriteFont gameFont;
        SpriteFont titleFont;
        int score;
        int highScore;
        int lives = 3;
        enum GameState
        {
            titleScreen,
            playingGame
        }
        GameState state = GameState.titleScreen;

        //Special variables for those crazy tomatoes
        int numberOfTomatoes = 20;
        int tomatoHeightLimit;
        int tomatoHeight;
        float tomatoStepFactor = .1f;
        Texture2D tomatoTexture;       

        //The sprites!
        GameSpriteStruct gameBackground;
        GameSpriteStruct cheese;
        GameSpriteStruct bread;
        GameSpriteStruct[] tomatoes;

        //Definition for what my sprite struct is
        struct GameSpriteStruct
        {
            public float WidthFactor;
            public float AspectRatio;
            public float X;
            public float Y;
            public float XSpeed;
            public float YSpeed;
            public float TicksToCrossScreen;
            public Texture2D Texture;
            public Rectangle Rectangle;
            public bool Visible;
        }

        /// <summary>
        /// Sets up all the aspects of a given sprite struct.
        /// </summary>
        /// <param name="sprite">The sprite to be set up. Passed by reference!</param>
        /// <param name="widthFactor">This is the scale you want for the sprite, relative to the display.</param>
        /// <param name="ticksToCrossScreen">How fast this sprite moves.</param>
        /// <param name="startX">Starting X coordinate.</param>
        /// <param name="startY">Starting Y coordinate.</param>
        /// <param name="visibility">Show the thing or not.</param>
        void setupSprite(ref GameSpriteStruct sprite, float widthFactor, float ticksToCrossScreen, float startX, float startY, bool visibility)
        {
            sprite.WidthFactor = widthFactor;
            sprite.TicksToCrossScreen = ticksToCrossScreen;
            sprite.AspectRatio = (float)sprite.Texture.Width / sprite.Texture.Height;
            sprite.Rectangle.Width = (int)((displayWidth * sprite.WidthFactor) + 0.5f);
            sprite.Rectangle.Height = (int)((sprite.Rectangle.Width / sprite.AspectRatio) + 0.5f);
            sprite.X = startX;
            sprite.Y = startY;
            sprite.XSpeed = displayWidth / sprite.TicksToCrossScreen;
            sprite.YSpeed = sprite.XSpeed;
            sprite.Visible = visibility;
        }

        /// <summary>
        /// Initializes game sprites and tomato array.
        /// </summary>
        void setupGame()
        {
            setupSprite(ref gameBackground, 1.0f, 0f, 0f, 0f, true);
            setupSprite(ref cheese, .05f, 200f, 200f, 200f, true);
            setupSprite(ref bread, .1f, 100f, displayWidth / 2, displayHeight / 1.2f, true);
            tomatoes = new GameSpriteStruct[numberOfTomatoes];
            float tomatoSpacing = displayWidth / numberOfTomatoes;

            tomatoHeightLimit = (int)displayHeight / 2;
            tomatoHeight = 0;

            for (int i = 0; i < numberOfTomatoes; i++)
            {
                tomatoes[i].Texture = tomatoTexture;
                setupSprite(ref tomatoes[i], .05f, 1000f, i * tomatoSpacing, tomatoHeight, true);
            }
        }


        /// <summary>
        /// When the cheese hits a tomato, make it disappear and increase the score.
        /// If all the tomatoes are gone, call resetCheese() and resetTomatoes() to increase difficulty.
        /// </summary>
        private void updateTomatoes()
        {
            bool noTomatoes = true;

            for (int i = 0; i < numberOfTomatoes; i++)
            {
                if (tomatoes[i].Visible)
                {
                    noTomatoes = false;
                    tomatoes[i].Rectangle.X = (int)tomatoes[i].X;
                    tomatoes[i].Rectangle.Y = (int)tomatoes[i].Y;
                    if (tomatoes[i].Rectangle.Intersects(cheese.Rectangle))
                    {
                        tomatoes[i].Visible = false;
                        cheese.YSpeed *= -1;
                        score += 10;
                        break;
                    }
                }
            }

            if (noTomatoes)
            {
                resetCheese();
                resetTomatoes();
            }
        }

        /// <summary>
        /// Allows the player to move the bread left and right with A and D.
        /// Bounces the cheese when it hits the bread.
        /// </summary>
        private void updateBread()
        {
            if (keys.IsKeyDown(Keys.D) && ((bread.X + bread.Rectangle.Width) < displayWidth))
            {
                bread.X = bread.X + bread.XSpeed;
            }

            if (keys.IsKeyDown(Keys.A) && (bread.X > 0))
            {
                bread.X = bread.X - bread.XSpeed;
            }

            bread.Rectangle.X = (int)(bread.X + 0.5f);
            bread.Rectangle.Y = (int)(bread.Y + 0.5f);
            if (cheese.Rectangle.Intersects(bread.Rectangle))
            {
                cheese.YSpeed *= -1;
            }
        }

        /// <summary>
        /// Move the cheese around, bouncing when it hits the edge of the screen.
        /// If we hit the bottom of the screen, lose a life.
        /// </summary>
        private void updateCheese()
        {
            cheese.X += cheese.XSpeed;
            cheese.Y += cheese.YSpeed;
            cheese.Rectangle.X = (int)(cheese.X + 0.5f);
            cheese.Rectangle.Y = (int)(cheese.Y + 0.5f);
            if ((cheese.X + cheese.Rectangle.Width) > displayWidth)
            {
                cheese.XSpeed *= -1;
            }

            if (cheese.X <= 0)
            {
                cheese.XSpeed *= -1;
            }

            if ((cheese.Y + cheese.Rectangle.Height) > displayHeight)
            {
                cheese.YSpeed *= -1;
                lives--;
            }

            if (cheese.Y <= 0)
            {
                cheese.YSpeed *= -1;
            }
        }


        /// <summary>
        /// Called when the player clears a wave of tomatoes. Brings the tomatoes closer and makes them all visible again.
        /// </summary>
        void resetTomatoes()
        {
            float tomatoSpacing = displayWidth / numberOfTomatoes; //make sure our tomatoes are evenly spaced across the display
            tomatoHeight += (int)(displayHeight * tomatoStepFactor); //bring the tomatoes down to increase difficulty

            //Reset to the top if we get too far down.
            if (tomatoHeight >= tomatoHeightLimit)
            {
                tomatoHeight = 0;
            }

            //Actually does the work to reset the tomatoes.
            for (int i = 0; i < numberOfTomatoes; i++)
            {
                tomatoes[i].Texture = tomatoTexture;
                setupSprite(ref tomatoes[i], .05f, 1000f, i * tomatoSpacing, tomatoHeight, true);
            }
        }

        /// <summary>
        /// Puts the cheese back in the middle of screen. Called when all tomatoes are cleared.
        /// </summary>
        void resetCheese()
        {
            cheese.X = displayWidth / 2;
            cheese.Y = displayHeight / 2;
        }

        void startGame()
        {
            lives = 3;
            score = 0;
            startCheese();
            startBread();
            startTomatoes();            
        }

        void startCheese()
        {
            cheese.X = displayWidth / 2;
            cheese.Y = displayHeight / 2;
            cheese.YSpeed = Math.Abs(cheese.YSpeed);
        }

        void startBread()
        {
            bread.X = displayWidth / 2;
        }

        void startTomatoes()
        {
            float tomatoSpacing = displayWidth / numberOfTomatoes; //make sure our tomatoes are evenly spaced across the display
            tomatoHeight = 0;
            for (int i = 0; i < numberOfTomatoes; i++)
            {
                tomatoes[i].Texture = tomatoTexture;
                setupSprite(ref tomatoes[i], .05f, 1000f, i * tomatoSpacing, tomatoHeight, true);
            }
        }

        void gameOver()
        {
            if (score > highScore)
            {
                highScore = score;
            }
            state = GameState.titleScreen;
        }

        /// <summary>
        /// Draws text to the screen
        /// </summary>
        /// <param name="text">The text to be written</param>
        /// <param name="font">The font used</param>
        /// <param name="color">Color of the text</param>
        /// <param name="x">Left edge of text</param>
        /// <param name="y">Top of text</param>
        void drawText(string text, SpriteFont font, Color color, float x, float y)
        {
            Vector2 stringVector = new Vector2(x, y);

            Color backColor = new Color(0, 0, 0, 20);
            Color sideColor = new Color(190, 190, 190);


            for (int i = 0; i < 5; i++)
            {
                spriteBatch.DrawString(font, text, stringVector, backColor);
                stringVector.X++;
                stringVector.Y++;
            }
            
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.DrawString(font, text, stringVector, sideColor);
                stringVector.X++;
                stringVector.Y++;                
            }

            spriteBatch.DrawString(font, text, stringVector, color);
        }


        public BreadandCheese()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            displayHeight = GraphicsDevice.Viewport.Height;
            displayWidth = GraphicsDevice.Viewport.Width;
            cheese.Rectangle.X = (int)cheese.X;
            cheese.Rectangle.Y = (int)cheese.Y;
            bread.Rectangle.X = (int)bread.X;
            bread.Rectangle.Y = (int)bread.Y;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            gameFont = Content.Load<SpriteFont>("Fonts/Calibri");
            titleFont = Content.Load<SpriteFont>("Fonts/TitleFont");
            gameBackground.Texture = Content.Load<Texture2D>("Images/Background");
            cheese.Texture = Content.Load<Texture2D>("Images/Cheese");
            bread.Texture = Content.Load<Texture2D>("Images/Bread");
            tomatoTexture = Content.Load<Texture2D>("Images/Tomato");
            setupGame();

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            keys = Keyboard.GetState();

            switch(state)
            {
                case GameState.playingGame:
                    updateCheese();
                    if (lives <= 0)
                    {
                        gameOver();                        
                        return;
                    }

                    updateBread();

                    updateTomatoes();
                    break;
                case GameState.titleScreen:
                    if (keys.IsKeyDown(Keys.Enter))
                    {
                        startGame();
                        state = GameState.playingGame;
                    }
                    break;
                 
            }




            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            switch(state)
            {
                case GameState.playingGame:
                    drawBackground();
                    drawCheese();
                    drawBread();
                    drawTomatoes();
                    drawText("Score: " + score.ToString(), gameFont, Color.Black, 50, displayHeight - 50);
                    drawText("Lives: " + lives.ToString(), gameFont, Color.Black, 150, displayHeight - 50);
                    break;
                case GameState.titleScreen:
                    drawTitle();
                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void drawBackground()
        {
            spriteBatch.Draw(gameBackground.Texture, gameBackground.Rectangle, Color.White);
        }

        private void drawTomatoes()
        {
            for (int i = 0; i < numberOfTomatoes; i++)
            {
                if (tomatoes[i].Visible)
                {
                    spriteBatch.Draw(tomatoes[i].Texture, tomatoes[i].Rectangle, Color.White);
                }
            }
        }

        private void drawBread()
        {
            if (bread.Visible)
            {
                spriteBatch.Draw(bread.Texture, bread.Rectangle, Color.White);
            }
        }

        private void drawCheese()
        {
            if (cheese.Visible)
            {
                spriteBatch.Draw(cheese.Texture, cheese.Rectangle, Color.White);
            }
        }

        private void drawTitle()
        {
            spriteBatch.Draw(gameBackground.Texture, gameBackground.Rectangle, Color.White);
            drawText("BREAD AND CHEESE", titleFont, Color.Black, 200, 200);
            drawText("Press 'Enter' to start", gameFont, Color.Black, 300, 300);
            drawText("High Score: " + highScore, gameFont, Color.Black, 320, 350);
        }
    }
}
