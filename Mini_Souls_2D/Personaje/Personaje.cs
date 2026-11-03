using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mini_Souls_2D.Enemigos;
using Mini_Souls_2D.herramientas;
using System;
using System.Collections;

namespace Mini_Souls_2D.personaje
{
    public class Personaje
    {
        public Texture2D TexturaMovimiento { get; set; }
        public Texture2D TexturaAtaque { get; set; }
        public Vector2 Posicion { get; set; }
        public float Velocidad { get; set; }
        public HitboxCircular Hitbox;
        public Action<int, Vector2> AlAtacar;

        public int Vida { get; private set; }
        public int VidaMaxima { get; }
        public bool EstaVivo => Vida > 0;
        public bool EstaAtacando { get; private set; }
        public bool EstaCubriendo { get; private set; }
        public bool EstaRodando => tiempoRodarRestante > 0f;
        public bool EstaHaciendoParry => tiempoParryRestante > 0f;
        public float CooldownParryRestante => cooldownParryRestante;

        private const float ActualizacionesPorSegundo = 60f;
        private const float DuracionAtaque = 0.64f;
        private const float MomentoImpacto = 0.32f;
        private const float AlcanceAtaque = 110f;
        private const int DañoAtaque = 1;
        private const float DuracionRodar = 0.20f;
        private const float MultiplicadorRodar = 3f;
        private const float DuracionParry = 0.5f;
        private const float CooldownParry = 1.5f;
        private const float Coseno60Grados = 0.5f;
        private const float PasoMaximoColision = 8f;

        private KeyboardState tecladoAnterior;
        private Vector2 direccionMirada = Vector2.UnitX;
        private Vector2 direccionRodar;
        private float tiempoAtaque;
        private float tiempoRodarRestante;
        private float tiempoParryRestante;
        private float cooldownParryRestante;
        private bool golpeRegistrado;

        public Personaje(Texture2D texturaMov, Texture2D texturaAtq,
            Vector2 posicionInicial, int vidaMaxima = 100)
        {
            if (vidaMaxima <= 0)
                throw new ArgumentOutOfRangeException(nameof(vidaMaxima));

            TexturaMovimiento = texturaMov;
            TexturaAtaque = texturaAtq;
            Posicion = posicionInicial;
            Velocidad = 5f;
            VidaMaxima = vidaMaxima;
            Vida = vidaMaxima;
            Hitbox = new HitboxCircular(Posicion, 40f);
        }

        public void Update(Rectangle[] paredesDelMapa, ArrayList enemigos,
            Matrix matrizCamara, GameTime gameTime)
        {
            Update(paredesDelMapa, enemigos, matrizCamara, gameTime,
                Keyboard.GetState(), Mouse.GetState());
        }

        public void Update(Rectangle[] paredesDelMapa, ArrayList enemigos,
            Matrix matrizCamara, GameTime gameTime,
            KeyboardState teclado, MouseState mouse)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(gameTime));

            bool pulsaEspacio = teclado.IsKeyDown(Keys.Space)
                && tecladoAnterior.IsKeyUp(Keys.Space);
            bool pulsaV = teclado.IsKeyDown(Keys.V)
                && tecladoAnterior.IsKeyUp(Keys.V);
            tecladoAnterior = teclado;

            Hitbox.Centro = Posicion;
            EstaCubriendo = false;
            if (!EstaVivo)
                return;

            Vector2 mouseMundo = Vector2.Transform(
                new Vector2(mouse.X, mouse.Y), Matrix.Invert(matrizCamara));
            Vector2 haciaMouse = mouseMundo - Posicion;
            if (haciaMouse.LengthSquared() > 0.000001f)
                direccionMirada = Vector2.Normalize(haciaMouse);

            tiempoParryRestante = Math.Max(0f, tiempoParryRestante - deltaTime);
            cooldownParryRestante = Math.Max(0f, cooldownParryRestante - deltaTime);

            if (EstaRodando)
            {
                ActualizarRodar(paredesDelMapa, deltaTime);
                return;
            }

            if (EstaAtacando)
            {
                Atacar(enemigos, deltaTime);
                return;
            }

            Vector2 movimiento = LeerMovimiento(teclado);
            if (pulsaEspacio && Rodar(movimiento))
            {
                ActualizarRodar(paredesDelMapa, deltaTime);
                return;
            }

            if (pulsaV)
                Parry();

            if (!EstaHaciendoParry && mouse.LeftButton == ButtonState.Pressed)
            {
                EstaAtacando = true;
                tiempoAtaque = 0f;
                golpeRegistrado = false;
                return;
            }

            Cubrir(mouse.RightButton == ButtonState.Pressed);
            Moverse(movimiento, paredesDelMapa, deltaTime);
        }

        private static Vector2 LeerMovimiento(KeyboardState teclado)
        {
            Vector2 movimiento = Vector2.Zero;
            if (teclado.IsKeyDown(Keys.W)) movimiento.Y -= 1f;
            if (teclado.IsKeyDown(Keys.S)) movimiento.Y += 1f;
            if (teclado.IsKeyDown(Keys.A)) movimiento.X -= 1f;
            if (teclado.IsKeyDown(Keys.D)) movimiento.X += 1f;

            return movimiento == Vector2.Zero
                ? Vector2.Zero : Vector2.Normalize(movimiento);
        }

        private void Moverse(Vector2 movimiento, Rectangle[] paredes, float deltaTime)
        {
         
            Desplazar(movimiento * Velocidad * ActualizacionesPorSegundo * deltaTime,
                paredes);
        }

        private void Atacar(ArrayList enemigos, float deltaTime)
        {
            tiempoAtaque += deltaTime;

            if (!golpeRegistrado && tiempoAtaque >= MomentoImpacto)
            {
                golpeRegistrado = true;
                if (enemigos != null)
                {
                    foreach (object elemento in enemigos.ToArray())
                    {
                        if (elemento is IEnemigo enemigo && enemigo.EstaVivo
                            && Vector2.DistanceSquared(Posicion, enemigo.Posicion)
                                < AlcanceAtaque * AlcanceAtaque)
                        {
                            enemigo.RecibirDaño(DañoAtaque);
                            AlAtacar?.Invoke(DañoAtaque, enemigo.Posicion);
                        }
                    }
                }
            }

            if (tiempoAtaque >= DuracionAtaque)
                EstaAtacando = false;
        }

        private void Cubrir(bool mantenerClickDerecho)
        {
            EstaCubriendo = mantenerClickDerecho && !EstaHaciendoParry;
        }

        private bool Rodar(Vector2 movimiento)
        {
            if (EstaHaciendoParry)
                return false;

            direccionRodar = movimiento == Vector2.Zero ? -direccionMirada : movimiento;
            tiempoRodarRestante = DuracionRodar;
            return true;
        }

        private void ActualizarRodar(Rectangle[] paredes, float deltaTime)
        {
            float tiempoPaso = Math.Min(deltaTime, tiempoRodarRestante);
            Vector2 desplazamiento = direccionRodar * Velocidad
                * ActualizacionesPorSegundo * MultiplicadorRodar * tiempoPaso;

            bool completo = Desplazar(desplazamiento, paredes);
            tiempoRodarRestante = completo
                ? Math.Max(0f, tiempoRodarRestante - tiempoPaso) : 0f;
        }

        private void Parry()
        {
            if (EstaHaciendoParry || cooldownParryRestante > 0f)
                return;

            tiempoParryRestante = DuracionParry;
            cooldownParryRestante = CooldownParry;
        }

        public bool RecibirDaño(int daño, IEnemigo atacante)
        {
            if (atacante == null)
                throw new ArgumentNullException(nameof(atacante));

            return RecibirDaño(daño, atacante.Posicion, atacante);
        }

        public bool RecibirDaño(int daño, Vector2 origenDelAtaque, IEnemigo atacante = null)
        {
            if (daño <= 0 || !EstaVivo || EstaRodando)
                return false;

            if (EstaEnSectorDefensivo(origenDelAtaque))
            {
                if (EstaHaciendoParry)
                {
                    atacante?.aturdir();
                    return false;
                }

                if (EstaCubriendo)
                    return false;
            }

            Vida = Math.Max(0, Vida - daño);
            if (!EstaVivo)
            {
                EstaAtacando = false;
                EstaCubriendo = false;
                tiempoParryRestante = 0f;
            }

            return true;
        }

        private bool EstaEnSectorDefensivo(Vector2 origenDelAtaque)
        {
            Vector2 haciaOrigen = origenDelAtaque - Posicion;
            if (haciaOrigen.LengthSquared() <= 0.000001f)
                return false;

            float producto = Vector2.Dot(direccionMirada, Vector2.Normalize(haciaOrigen));
            return producto >= Coseno60Grados - 0.00001f;
        }

        private bool Desplazar(Vector2 desplazamiento, Rectangle[] paredes)
        {
            int pasos = Math.Max(1, (int)Math.Ceiling(
                desplazamiento.Length() / PasoMaximoColision));
            Vector2 paso = desplazamiento / pasos;

            for (int i = 0; i < pasos; i++)
            {
                Hitbox.Centro = Posicion + paso;
                if (HayColision(paredes))
                {
                    Hitbox.Centro = Posicion;
                    return false;
                }

                Posicion = Hitbox.Centro;
            }

            return true;
        }

        private bool HayColision(Rectangle[] paredes)
        {
            if (paredes == null)
                return false;

            foreach (Rectangle pared in paredes)
                if (Hitbox.IntersectaConRectangulo(pared))
                    return true;

            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (TexturaMovimiento == null)
                return;

            int ancho = TexturaMovimiento.Width / 8;
            int alto = TexturaMovimiento.Height / 4;
            Rectangle recorte = new Rectangle(0, 0, ancho, alto);
            Vector2 centro = new Vector2(ancho / 2f, alto / 2f);
            float escala = 100f / MathHelper.Max(ancho, alto);

            spriteBatch.Draw(TexturaMovimiento, Posicion, recorte, Color.White,
                0f, centro, escala, SpriteEffects.None, 0f);
        }
    }
}