using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mini_Souls_2D
{
    public class Dummy
    {
        public Texture2D Textura { get; set; }
        public Vector2 Posicion { get; set; }
        public int Vida { get; set; } = 100;
        public bool EstaVivo => Vida > 0;
        public HitboxCircular Hitbox;

        private Texture2D texturaVida;

        public Dummy(Texture2D textura, Texture2D texturaVida, Vector2 posicionInicial)
        {
            Textura = textura;
            this.texturaVida = texturaVida;
            Posicion = posicionInicial;
            Hitbox = new HitboxCircular(Posicion, 35f);
        }

        public void RecibirDaño(int cantidad)
        {
            Vida -= cantidad;
            if (Vida < 0) Vida = 0;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (EstaVivo)
            {
                Vector2 centroTextura = new Vector2(Textura.Width / 2f, Textura.Height / 2f);
                float escalaDummy = 80f / MathHelper.Max(Textura.Width, Textura.Height);
                spriteBatch.Draw(Textura, Posicion, null, Color.White, 0f, centroTextura, escalaDummy, SpriteEffects.None, 0f);

                int valorVida = MathHelper.Clamp(Vida, 1, 99);
                int columnas = 11;
                int filas = 9;
                int anchoVida = texturaVida.Width / columnas;
                int altoVida = texturaVida.Height / filas;

                int indice = valorVida - 1;
                int fila = indice / columnas;
                int columna = indice % columnas;

                Rectangle origenVida = new Rectangle(columna * anchoVida, fila * altoVida, anchoVida, altoVida);
                Vector2 centroVida = new Vector2(anchoVida / 2f, altoVida / 2f);
                Vector2 posVida = new Vector2(Posicion.X, Posicion.Y - 50f);

                spriteBatch.Draw(texturaVida, posVida, origenVida, Color.White, 0f, centroVida, 0.6f, SpriteEffects.None, 0f);
            }
        }
    }
}