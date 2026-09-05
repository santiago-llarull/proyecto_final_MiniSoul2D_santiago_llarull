using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Mini_Souls_2D
{
    public class Personaje
    {
        public Texture2D TexturaMovimiento { get; set; }
        public Texture2D TexturaAtaque { get; set; }
        public Vector2 Posicion { get; set; }
        public float Velocidad { get; set; }
        public HitboxCircular Hitbox;
        public Action<int, Vector2> AlAtacar;

        private int totalColumnas = 8;
        private int totalFilas = 4;
        private int frameActual = 0;
        private int filaActual = 0;
        private float tiempoPorFrame = 0.08f;
        private float tiempoAcumulado = 0f;
        private bool enMovimiento = false;
        private bool estaAtacando = false;
        private bool golpeRegistrado = false;
        private Random rng = new Random();

        public Personaje(Texture2D texturaMov, Texture2D texturaAtq, Vector2 posicionInicial)
        {
            TexturaMovimiento = texturaMov;
            TexturaAtaque = texturaAtq;
            Posicion = posicionInicial;
            Velocidad = 5f;
            Hitbox = new HitboxCircular(Posicion, 40f);
        }

        public void Update(Rectangle[] paredesDelMapa, Dummy dummy, Matrix matrizCamara, GameTime gameTime)
        {
            KeyboardState teclado = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            Vector2 movimiento = Vector2.Zero;

            if (estaAtacando)
            {
                tiempoAcumulado += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (tiempoAcumulado >= tiempoPorFrame)
                {
                    frameActual++;
                    tiempoAcumulado = 0f;

                    if (frameActual == 4 && !golpeRegistrado)
                    {
                        if (dummy != null && dummy.EstaVivo)
                        {
                            float distanciaAlDummy = Vector2.Distance(Posicion, dummy.Posicion);
                            if (distanciaAlDummy < 110f)
                            {
                                int daño = rng.Next(10, 31);
                                dummy.RecibirDaño(daño);
                                AlAtacar?.Invoke(daño, dummy.Posicion);
                            }
                        }
                        golpeRegistrado = true;
                    }

                    if (frameActual >= totalColumnas)
                    {
                        estaAtacando = false;
                        frameActual = 0;
                    }
                }
                return;
            }

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                estaAtacando = true;
                golpeRegistrado = false;
                frameActual = 0;
                tiempoAcumulado = 0f;

                Vector2 posicionMousePantalla = new Vector2(mouse.X, mouse.Y);
                Vector2 posicionMouseMundo = Vector2.Transform(posicionMousePantalla, Matrix.Invert(matrizCamara));
                Vector2 direccion = posicionMouseMundo - Posicion;
                float angulo = (float)Math.Atan2(direccion.Y, direccion.X);

                if (angulo >= -MathHelper.PiOver4 && angulo < MathHelper.PiOver4) filaActual = 2;
                else if (angulo >= MathHelper.PiOver4 && angulo < 3 * MathHelper.PiOver4) filaActual = 0;
                else if (angulo >= 3 * MathHelper.PiOver4 || angulo < -3 * MathHelper.PiOver4) filaActual = 1;
                else filaActual = 3;

                return;
            }

            enMovimiento = false;

            if (teclado.IsKeyDown(Keys.S)) { movimiento.Y += 1; filaActual = 0; }
            if (teclado.IsKeyDown(Keys.A)) { movimiento.X -= 1; filaActual = 1; }
            if (teclado.IsKeyDown(Keys.D)) { movimiento.X += 1; filaActual = 2; }
            if (teclado.IsKeyDown(Keys.W)) { movimiento.Y -= 1; filaActual = 3; }

            if (movimiento != Vector2.Zero)
            {
                enMovimiento = true;
                movimiento.Normalize();

                Vector2 posicionFutura = Posicion + (movimiento * Velocidad);
                Hitbox.Centro = posicionFutura;

                bool hayColision = false;
                foreach (Rectangle pared in paredesDelMapa)
                {
                    if (Hitbox.IntersectaConRectangulo(pared))
                    {
                        hayColision = true;
                        break;
                    }
                }

                if (!hayColision) Posicion = posicionFutura;
                else Hitbox.Centro = Posicion;
            }

            if (enMovimiento)
            {
                tiempoAcumulado += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (tiempoAcumulado >= tiempoPorFrame)
                {
                    frameActual++;
                    if (frameActual >= totalColumnas) frameActual = 0;
                    tiempoAcumulado = 0f;
                }
            }
            else
            {
                frameActual = 0;
                tiempoAcumulado = 0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D texturaActual = estaAtacando ? TexturaAtaque : TexturaMovimiento;

            int anchoFrame = texturaActual.Width / totalColumnas;
            int altoFrame = texturaActual.Height / totalFilas;

            Rectangle recorteFrame = new Rectangle(
                frameActual * anchoFrame,
                filaActual * altoFrame,
                anchoFrame,
                altoFrame
            );

            Vector2 centroTextura = new Vector2(anchoFrame / 2f, altoFrame / 2f);
            float escalaDeseada = 100f / MathHelper.Max(anchoFrame, altoFrame);

            spriteBatch.Draw(
                texturaActual,
                Posicion,
                recorteFrame,
                Color.White,
                0f,
                centroTextura,
                escalaDeseada,
                SpriteEffects.None,
                0f
            );
        }
    }
}