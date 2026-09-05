using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace Mini_Souls_2D
{
    public class Principal
    {
        private Camara2D camara;
        private Escenario mapa;
        private Personaje jugador;
        private Dummy dummyPrueba;
        private int anchoPantalla;
        private int altoPantalla;
        private Texture2D texturaVida;
        private List<ParticulaDaño> particulas;

        public void LoadContent(ContentManager content, GraphicsDeviceManager graphics)
        {
            anchoPantalla = graphics.PreferredBackBufferWidth;
            altoPantalla = graphics.PreferredBackBufferHeight;
            camara = new Camara2D();
            particulas = new List<ParticulaDaño>();

            Texture2D imagenFondo = content.Load<Texture2D>("esenario");
            Texture2D imagenMovimiento = content.Load<Texture2D>("movimiento");
            Texture2D imagenAtaque = content.Load<Texture2D>("ataque");
            Texture2D imagenDummy = content.Load<Texture2D>("dummy");
            texturaVida = content.Load<Texture2D>("vida");

            mapa = new Escenario(imagenFondo);
            Vector2 centroDelMapa = new Vector2(mapa.TexturaFondo.Width / 2f, mapa.TexturaFondo.Height / 2f);

            jugador = new Personaje(imagenMovimiento, imagenAtaque, centroDelMapa);
            jugador.AlAtacar = (daño, pos) =>
            {
                particulas.Add(new ParticulaDaño(texturaVida, pos, daño));
            };

            dummyPrueba = new Dummy(imagenDummy, texturaVida, centroDelMapa + new Vector2(150f, 0f));
        }

        public void Update(GameTime gameTime)
        {
            jugador.Update(mapa.BordesHitbox, dummyPrueba, camara.Transformacion, gameTime);
            camara.Actualizar(jugador.Posicion, anchoPantalla, altoPantalla);

            for (int i = particulas.Count - 1; i >= 0; i--)
            {
                particulas[i].Update(gameTime);
                if (!particulas[i].Activa)
                {
                    particulas.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: camara.Transformacion, samplerState: SamplerState.PointClamp);

            mapa.Draw(spriteBatch);
            dummyPrueba.Draw(spriteBatch);
            jugador.Draw(spriteBatch);

            foreach (var particula in particulas)
            {
                particula.Draw(spriteBatch);
            }

            spriteBatch.End();
        }
    }
}