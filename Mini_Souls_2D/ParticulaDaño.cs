using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mini_Souls_2D
{
    public class ParticulaDaño
    {
        public Vector2 Posicion { get; private set; }
        public bool Activa { get; private set; }

        private int valor;
        private float tiempoVida;
        private Texture2D textura;

        public ParticulaDaño(Texture2D textura, Vector2 posicion, int valor)
        {
            this.textura = textura;
            Posicion = posicion;
            this.valor = MathHelper.Clamp(valor, 1, 99);
            tiempoVida = 1.0f;
            Activa = true;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            tiempoVida -= dt;
            Posicion = new Vector2(Posicion.X, Posicion.Y - (40f * dt));

            if (tiempoVida <= 0f)
            {
                Activa = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Activa) return;

            int columnas = 11;
            int filas = 9;
            int ancho = textura.Width / columnas;
            int alto = textura.Height / filas;

            int indice = valor - 1;
            int fila = indice / columnas;
            int columna = indice % columnas;

            Rectangle origen = new Rectangle(columna * ancho, fila * alto, ancho, alto);
            Vector2 centro = new Vector2(ancho / 2f, alto / 2f);
            float opacidad = MathHelper.Clamp(tiempoVida, 0f, 1f);

            spriteBatch.Draw(textura, Posicion, origen, Color.White * opacidad, 0f, centro, 0.7f, SpriteEffects.None, 0f);
        }
    }
}