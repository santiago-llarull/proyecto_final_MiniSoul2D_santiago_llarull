using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mini_Souls_2D.Enemigos;
using Mini_Souls_2D.herramientas;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mini_Souls_2D.personaje
{
    public class Personaje
    {
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

        private enum Animacion
        {
            Reposo,
            Caminar,
            Ataque,
            AtaqueAvance,
            Guardia,
            GuardiaAvance,
            Rodar
        }

        private const int Columnas = 8;
        private const int Filas = 4;
        private const float TamañoDibujo = 200f;
        private const int CuadroImpacto = 3;
        private const float SegundosPorCuadro = 0.08f;
        private const float ActualizacionesPorSegundo = 60f;
        private const float DuracionAtaque = 0.64f;
        private const float MomentoImpacto = 0.32f;
        private const float AlcanceAtaque = 110f;
        private const float DistanciaAtaqueAvance = 48f;
        private const int DañoAtaque = 1;
        private const float DuracionRodar = 0.20f;
        private const float MultiplicadorRodar = 3f;
        private const float DuracionParry = 0.5f;
        private const float CooldownParry = 1.5f;
        private const float Coseno60Grados = 0.5f;
        private const float PasoMaximoColision = 8f;

        private static readonly TimeSpan CooldownAtaque = TimeSpan.FromSeconds(1);

        // Cada fila de la hoja corresponde a una dirección sobre un solo eje.
        private static readonly Vector2[] DireccionesPorFila =
        {
            Vector2.UnitY,
            -Vector2.UnitY,
            -Vector2.UnitX,
            Vector2.UnitX
        };

        private readonly Dictionary<Animacion, Texture2D> texturas;
        private Animacion animacionActual = Animacion.Reposo;
        private int filaActual;
        private int cuadroActual;
        private float tiempoCiclo;
        private KeyboardState tecladoAnterior;
        private Vector2 direccionMirada = Vector2.UnitX;
        private Vector2 direccionAtaque;
        private Vector2 direccionRodar;
        private float tiempoAtaque;
        private TimeSpan cooldownAtaqueRestante;
        private float tiempoRodarRestante;
        private float tiempoParryRestante;
        private float cooldownParryRestante;
        private bool golpeRegistrado;

        public Personaje(Texture2D reposo, Texture2D caminar, Texture2D ataque,
            Texture2D ataqueAvance, Texture2D guardia, Texture2D guardiaAvance, Texture2D rodar,
            Vector2 posicionInicial, int vidaMaxima = 100)
        {
            if (vidaMaxima <= 0)
                throw new ArgumentOutOfRangeException(nameof(vidaMaxima));

            texturas = new Dictionary<Animacion, Texture2D>
            {
                [Animacion.Reposo] = reposo,
                [Animacion.Caminar] = caminar,
                [Animacion.Ataque] = ataque,
                [Animacion.AtaqueAvance] = ataqueAvance,
                [Animacion.Guardia] = guardia,
                [Animacion.GuardiaAvance] = guardiaAvance,
                [Animacion.Rodar] = rodar
            };

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
            cooldownAtaqueRestante -= gameTime.ElapsedGameTime;
            if (cooldownAtaqueRestante < TimeSpan.Zero)
                cooldownAtaqueRestante = TimeSpan.Zero;

            if (EstaRodando)
            {
                ActualizarRodar(paredesDelMapa, deltaTime);
                return;
            }

            if (EstaAtacando)
            {
                Atacar(enemigos, paredesDelMapa, deltaTime);
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

            if (!EstaHaciendoParry && cooldownAtaqueRestante == TimeSpan.Zero
                && mouse.LeftButton == ButtonState.Pressed)
            {
                IniciarAtaque(movimiento != Vector2.Zero);
                return;
            }

            Cubrir(mouse.RightButton == ButtonState.Pressed);
            Vector2 posicionAnterior = Posicion;
            Moverse(movimiento, paredesDelMapa, deltaTime);

            bool seMovio = Posicion != posicionAnterior;
            if (EstaCubriendo || EstaHaciendoParry)
            {
                AnimarCiclo(seMovio ? Animacion.GuardiaAvance : Animacion.Guardia,
                    direccionMirada, deltaTime);
            }
            else
            {
                AnimarCiclo(seMovio ? Animacion.Caminar : Animacion.Reposo,
                    seMovio ? movimiento : Vector2.Zero, deltaTime);
            }
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

        private void IniciarAtaque(bool conAvance)
        {
            EstaAtacando = true;
            tiempoAtaque = 0f;
            cooldownAtaqueRestante = CooldownAtaque;
            golpeRegistrado = false;
            filaActual = ObtenerFila(direccionMirada);
            direccionAtaque = DireccionesPorFila[filaActual];
            animacionActual = conAvance ? Animacion.AtaqueAvance : Animacion.Ataque;
            cuadroActual = 0;
        }

        private void Atacar(ArrayList enemigos, Rectangle[] paredes, float deltaTime)
        {
            float tiempoAnterior = tiempoAtaque;
            tiempoAtaque = Math.Min(DuracionAtaque, tiempoAtaque + deltaTime);

            if (animacionActual == Animacion.AtaqueAvance && tiempoAnterior < MomentoImpacto)
            {
                float tiempoAvance = Math.Min(tiempoAtaque, MomentoImpacto) - tiempoAnterior;
                Vector2 avance = direccionAtaque * (DistanciaAtaqueAvance * tiempoAvance / MomentoImpacto);
                Desplazar(avance, paredes);
            }

            // El cuarto cuadro muestra el corte y empieza en el momento del impacto.
            if (tiempoAtaque < MomentoImpacto)
            {
                cuadroActual = CuadroDeProgreso(tiempoAtaque / MomentoImpacto, CuadroImpacto);
            }
            else
            {
                float progreso = (tiempoAtaque - MomentoImpacto) / (DuracionAtaque - MomentoImpacto);
                cuadroActual = CuadroImpacto + CuadroDeProgreso(progreso, Columnas - CuadroImpacto);
            }

            if (!golpeRegistrado && tiempoAtaque >= MomentoImpacto)
            {
                golpeRegistrado = true;
                if (enemigos != null)
                {
                    foreach (object elemento in enemigos.ToArray())
                    {
                        if (elemento is Enemigo enemigo && enemigo.EstaVivo
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

            Vector2 direccionElegida = movimiento == Vector2.Zero ? -direccionMirada : movimiento;
            filaActual = ObtenerFila(direccionElegida);
            direccionRodar = DireccionesPorFila[filaActual];
            tiempoRodarRestante = DuracionRodar;
            animacionActual = Animacion.Rodar;
            cuadroActual = 0;
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
            cuadroActual = CuadroDeProgreso(1f - tiempoRodarRestante / DuracionRodar, Columnas);
        }

        private void Parry()
        {
            if (EstaHaciendoParry || cooldownParryRestante > 0f)
                return;

            tiempoParryRestante = DuracionParry;
            cooldownParryRestante = CooldownParry;
        }

        public bool RecibirDaño(int daño, Enemigo atacante)
        {
            if (atacante == null)
                throw new ArgumentNullException(nameof(atacante));

            return RecibirDaño(daño, atacante.Posicion, atacante);
        }

        public bool RecibirDaño(int daño, Vector2 origenDelAtaque, Enemigo atacante = null)
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
                animacionActual = Animacion.Reposo;
                cuadroActual = 0;
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
            if (desplazamiento == Vector2.Zero)
                return true;

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

        private void AnimarCiclo(Animacion animacion, Vector2 direccion, float deltaTime)
        {
            float duracion = SegundosPorCuadro * Columnas;
            tiempoCiclo = animacionActual == animacion ? (tiempoCiclo + deltaTime) % duracion : 0f;
            animacionActual = animacion;
            cuadroActual = CuadroDeProgreso(tiempoCiclo / duracion, Columnas);

            if (direccion != Vector2.Zero)
                filaActual = ObtenerFila(direccion);
        }

        private static int ObtenerFila(Vector2 direccion)
        {
            // Las hojas están ordenadas: abajo, arriba, izquierda, derecha.
            if (Math.Abs(direccion.X) >= Math.Abs(direccion.Y))
                return direccion.X < 0f ? 2 : 3;

            return direccion.Y < 0f ? 1 : 0;
        }

        private static int CuadroDeProgreso(float progreso, int cantidad)
        {
            return Math.Min(cantidad - 1, (int)(progreso * cantidad));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D textura = texturas[animacionActual];
            int ancho = textura.Width / Columnas;
            int alto = textura.Height / Filas;
            Rectangle recorte = new Rectangle(cuadroActual * ancho, filaActual * alto, ancho, alto);
            Vector2 centro = new Vector2(ancho / 2f, alto / 2f);
            float escala = TamañoDibujo / MathHelper.Max(ancho, alto);

            spriteBatch.Draw(textura, Posicion, recorte, Color.White,
                0f, centro, escala, SpriteEffects.None, 0f);
        }
    }
}
