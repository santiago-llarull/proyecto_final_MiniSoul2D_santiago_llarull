

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Mini_Souls_2D.Enemigos;
using Mini_Souls_2D.herramientas;
using System.Collections;

using Mini_Souls_2D.Enemigos;
using Mini_Souls_2D.herramientas;
using Mini_Souls_2D.personaje;

namespace Mini_Souls_2D.principal
{
    public class Principal
    {
        private Texture2D imagenReposo;
        private Texture2D imagenCaminar;
        private Texture2D imagenAtaque;
        private Texture2D imagenAtaqueAvance;
        private Texture2D imagenRodar;
        private Texture2D imagenGuardia;
        private Texture2D imagenGuardiaAvance;

        private readonly ArrayList enemigos = new ArrayList();

        private Camara2D camara;
        private Escenario mapa;
        private Personaje jugador;
        private int anchoPantalla;
        private int altoPantalla;

        public void LoadContent(ContentManager content, GraphicsDeviceManager graphics)
        {
            anchoPantalla = graphics.PreferredBackBufferWidth;
            altoPantalla = graphics.PreferredBackBufferHeight;
            camara = new Camara2D();
            Texture2D imagenFondo = content.Load<Texture2D>("esenario");
            mapa = new Escenario(imagenFondo);
            cargarTexturasPersonaje(content, graphics);

            Vector2 centroDelMapa = new Vector2(mapa.TexturaFondo.Width / 2f, mapa.TexturaFondo.Height / 2f);

            jugador = new Personaje(
                reposo: imagenReposo,
                caminar: imagenCaminar,
                ataque: imagenAtaque,
                ataqueAvance: imagenAtaqueAvance,
                guardia: imagenGuardia,
                guardiaAvance: imagenGuardiaAvance,
                rodar: imagenRodar,
                posicionInicial: centroDelMapa);

            camara.Actualizar(jugador.Posicion, anchoPantalla, altoPantalla);
        }

        public void Update(GameTime gameTime)
        {
            jugador.Update(mapa.BordesHitbox, enemigos, camara.Transformacion, gameTime);
            camara.Actualizar(jugador.Posicion, anchoPantalla, altoPantalla);


        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: camara.Transformacion, samplerState: SamplerState.PointClamp);

            mapa.Draw(spriteBatch);
            jugador.Draw(spriteBatch);



            spriteBatch.End();
        }



        public void cargarTexturasPersonaje(ContentManager content, GraphicsDeviceManager graphics)
        {
            imagenReposo = content.Load<Texture2D>("reposo");
            imagenCaminar = content.Load<Texture2D>("caminar");
            imagenAtaque = content.Load<Texture2D>("ataque");
            imagenAtaqueAvance = content.Load<Texture2D>("ataque_avance");
            imagenRodar = content.Load<Texture2D>("rodar");
            imagenGuardia = content.Load<Texture2D>("guardia");
            imagenGuardiaAvance = content.Load<Texture2D>("guardia_avance");
        }
    }
}
