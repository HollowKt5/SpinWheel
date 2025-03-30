using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace Something
{//可通过设置随机初始速度来确保每次转动时间不同
 //或者按键时间长短来决定速度，也生成一个根据时间长短的随机速度 formula: timer*random.Next(30,100);
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        Wheel wheel;
        Vector2 center;
        const int radius = 300;
        bool isSpinning = false;
        bool canReset = false;
        bool allowSpin = true;
        Random random = new Random();

        // 重置按钮参数
        Rectangle resetButtonRect;
        const int buttonWidth = 120;
        const int buttonHeight = 50;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
        }

        protected override void Initialize()
        {
            DrawingExtensions.Initialize(GraphicsDevice);
            resetButtonRect = new Rectangle(
                graphics.PreferredBackBufferWidth - buttonWidth - 20,
                graphics.PreferredBackBufferHeight - buttonHeight - 20,
                buttonWidth,
                buttonHeight
            );
            base.Initialize();
        }

        protected override void LoadContent()
        {          
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //font = CreateDefaultFont(GraphicsDevice);
            font = Content.Load<SpriteFont>("PixelFont");

            var segments = new List<WheelSegment>
            {
                new WheelSegment("FOOT", Color.Red),
                new WheelSegment("FOOT", Color.Green),//FETISH
                new WheelSegment("FOOT", Color.Orange),
                new WheelSegment("FOOT", Color.Green),
                new WheelSegment("FAKE", Color.Yellow),
                new WheelSegment("FOOT", Color.Green),
                new WheelSegment("SHA", Color.Blue),//SHA
                new WheelSegment("FOOT", Color.Green),
                new WheelSegment("FOOT", Color.Indigo),//MATURE
                new WheelSegment("FOOT", Color.Green),
                new WheelSegment("FOOT", Color.Violet),//GERMANY EUROPEAN
                new WheelSegment("FOOT", Color.Green),
                new WheelSegment("FOOT", Color.Pink),
                new WheelSegment("FOOT", Color.Green),
                new WheelSegment("FOOT", Color.Cyan),

            };

            wheel = new Wheel(segments, radius, font);
            center = new Vector2(graphics.PreferredBackBufferWidth / 2,
                               graphics.PreferredBackBufferHeight / 2);
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var mouseState = Mouse.GetState();
            Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);

            // 重置按钮逻辑（新增）
            if (canReset && mouseState.LeftButton == ButtonState.Pressed)
            {
                if (resetButtonRect.Contains(mousePos))
                {
                    //wheel.ForceAlignToSection();
                    canReset = false;
                    allowSpin = true;
                }
            }

            // 旋转按钮逻辑（修改）
            if (allowSpin && !isSpinning && mouseState.LeftButton == ButtonState.Pressed)
            {
                if (Vector2.Distance(mousePos, center) < 50)
                {
                    StartSpin();
                    allowSpin = false;
                }
            }

            if (isSpinning)
            {
                wheel.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                if (wheel.RotationSpeed <= 0)
                {
                    isSpinning = false;
                    canReset = true; // 新增状态转换
                }
            }

            base.Update(gameTime);
        }



        void StartSpin()
        {
           
            wheel.StartRotation();
            isSpinning = true;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp);
            wheel.Draw(spriteBatch, center);
            DrawStartButton();
            DrawPointer();
            DrawResetButton();
            spriteBatch.End();

            base.Draw(gameTime);
        }

        void DrawPointer()
        {
            Vector2 tip = center + new Vector2(0, -radius - 20);
            Vector2 baseLeft = center + new Vector2(-15, -radius + 20);
            Vector2 baseRight = center + new Vector2(15, -radius + 20);

            spriteBatch.DrawLine(tip, baseLeft, Color.Gold, 3);
            spriteBatch.DrawLine(tip, baseRight, Color.Gold, 3);
            spriteBatch.DrawLine(baseLeft, baseRight, Color.Gold, 3);
        }

        void DrawStartButton()
        {
            Color btnColor = allowSpin ? Color.Gold : Color.Gray;
            spriteBatch.DrawCircle(center, 50, 32, btnColor, 3);

            string btnText = allowSpin ? "SPIN" : "LOCKED";
            Vector2 textSize = font.MeasureString(btnText) * 0.6f;
            Color textColor = allowSpin ? Color.White : Color.DarkGray;

            spriteBatch.DrawString(font, btnText, center - textSize / 2, textColor, 0,
                                 Vector2.Zero, 0.6f, SpriteEffects.None, 0);
        }

        void DrawResetButton()
        {
            Color btnColor = canReset ? Color.LimeGreen : Color.Gray;
            Color textColor = canReset ? Color.White : Color.DarkGray;

            // 绘制按钮背景
            spriteBatch.Draw(DrawingExtensions.GetPixelTexture(), resetButtonRect, btnColor);

            // 绘制边框
            spriteBatch.DrawRectangle(
                new Vector2(resetButtonRect.X, resetButtonRect.Y),
                resetButtonRect.Width,
                resetButtonRect.Height,
                Color.White,
                2
            );

            // 绘制文字
            string btnText = canReset ? "RESET" : "LOCKED";
            Vector2 textSize = font.MeasureString(btnText);
            Vector2 textPos = new Vector2(
                resetButtonRect.Center.X - textSize.X / 2,
                resetButtonRect.Center.Y - textSize.Y / 2
            );

            spriteBatch.DrawString(font, btnText, textPos, textColor);
        }
    }

    public class Wheel
    {
        public float RotationAngle { get; private set; }
        public float RotationSpeed { get; private set; }
        private List<WheelSegment> segments;
        private int radius;
        private SpriteFont font;

        public Wheel(List<WheelSegment> segments, int radius, SpriteFont font)
        {
            this.segments = segments;
            this.radius = radius;
            this.font = font;
        }

        // 新增方法
        public void ForceAlignToSection()
        {
            float segmentSize = 360f / segments.Count;
            float finalAngle = RotationAngle % 360;
            float adjustAngle = (segmentSize - (finalAngle % segmentSize)) % segmentSize;
            RotationAngle += adjustAngle;
        }

        public void StartRotation()
        {
            Random random = new Random();
            RotationSpeed = 300f;
            RotationSpeed += random.Next(400, 1000);
        }

        // 修改更新逻辑
        public void Update(float deltaTime)
        {
            if (RotationSpeed > 0)
            {
                RotationAngle += RotationSpeed * deltaTime;
                RotationSpeed = MathHelper.Lerp(RotationSpeed, 0, deltaTime * 0.5f);
            }
            if (RotationSpeed < 5f)
                RotationSpeed = 0;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 center)
        {
            float segmentAngle = 360f / segments.Count;

            for (int i = 0; i < segments.Count; i++)
            {
                float startAngle = MathHelper.ToRadians(i * segmentAngle + RotationAngle);
                float endAngle = MathHelper.ToRadians((i + 1) * segmentAngle + RotationAngle);

                spriteBatch.DrawSegment(center, radius, startAngle, endAngle, segments[i].Color);
                DrawSegmentText(spriteBatch, center, startAngle + MathHelper.Pi / segments.Count,
                              segments[i].Text);
            }

            spriteBatch.DrawCircle(center, radius, 64, Color.White, 3);
        }

        private void DrawSegmentText(SpriteBatch sb, Vector2 center, float angle, string text)
        {
            Vector2 textPos = center + Vector2.Normalize(new Vector2(
                (float)Math.Cos(angle),
                (float)Math.Sin(angle))) * (radius * 0.7f);

            float textRotation = angle + MathHelper.Pi / 2;
            Vector2 textOrigin = font.MeasureString(text) / 2;

            sb.DrawString(font, text, textPos, Color.Black,
                        textRotation, textOrigin, 0.6f, SpriteEffects.None, 0);
        }
    }

    public static class DrawingExtensions
    {
        private static Texture2D _pixel;

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public static Texture2D GetPixelTexture() => _pixel;

        public static void DrawSegment(this SpriteBatch sb, Vector2 center, float radius,
                                float startAngle, float endAngle, Color color)
        {
            const int segments = 32;
            var vertices = new List<VertexPositionColor>();

            Vector2 prevPos = center + new Vector2(
                (float)Math.Cos(startAngle) * radius,
                (float)Math.Sin(startAngle) * radius);

            for (int i = 1; i <= segments; i++)
            {
                float lerp = i / (float)segments;
                float angle = MathHelper.Lerp(startAngle, endAngle, lerp);
                Vector2 pos = center + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius);

                vertices.Add(new VertexPositionColor(new Vector3(center, 0), color));
                vertices.Add(new VertexPositionColor(new Vector3(prevPos, 0), color));
                vertices.Add(new VertexPositionColor(new Vector3(pos, 0), color));

                prevPos = pos;
            }

            if (vertices.Count > 0)
            {
                using (BasicEffect basicEffect = new BasicEffect(sb.GraphicsDevice))
                {
                    basicEffect.VertexColorEnabled = true;
                    basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                        0, sb.GraphicsDevice.Viewport.Width,
                        sb.GraphicsDevice.Viewport.Height, 0,
                        0, 1);

                    foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        sb.GraphicsDevice.DrawUserPrimitives(
                            PrimitiveType.TriangleList,
                            vertices.ToArray(),
                            0,
                            vertices.Count / 3);
                    }
                }
            }
        }

        public static void DrawCircle(this SpriteBatch sb, Vector2 center, float radius,
                                     int sides, Color color, float thickness)
        {
            Vector2[] points = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = MathHelper.TwoPi * i / sides;
                points[i] = center + radius * new Vector2((float)Math.Cos(angle),
                                                         (float)Math.Sin(angle));
            }

            for (int i = 0; i < sides; i++)
            {
                Vector2 start = points[i];
                Vector2 end = points[(i + 1) % sides];
                sb.DrawLine(start, end, color, thickness);
            }
        }

        public static void DrawLine(this SpriteBatch sb, Vector2 start, Vector2 end,
                                   Color color, float thickness)
        {
            Vector2 delta = end - start;
            float rotation = (float)Math.Atan2(delta.Y, delta.X);
            sb.Draw(_pixel, start, null, color, rotation,
                   new Vector2(0, 0.5f),
                   new Vector2(delta.Length(), thickness),
                   SpriteEffects.None, 0);
        }

        public static void DrawRectangle(this SpriteBatch sb, Vector2 position,
            int width, int height, Color color, float thickness)
        {
            sb.DrawLine(position, new Vector2(position.X + width, position.Y), color, thickness);
            sb.DrawLine(new Vector2(position.X + width, position.Y),
                new Vector2(position.X + width, position.Y + height), color, thickness);
            sb.DrawLine(new Vector2(position.X, position.Y + height),
                new Vector2(position.X + width, position.Y + height), color, thickness);
            sb.DrawLine(position, new Vector2(position.X, position.Y + height), color, thickness);
        }
    }

    public class WheelSegment
    {
        public string Text { get; set; }
        public Color Color { get; set; }

        public WheelSegment(string text, Color color)
        {
            Text = text;
            Color = color;
        }
    }
}











//font = Content.Load<SpriteFont>("PixelFont");