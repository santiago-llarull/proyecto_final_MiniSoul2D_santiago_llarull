using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mini_Souls_2D
{
    public class Escenario
    {
        public Texture2D TexturaFondo { get; private set; }
        public Rectangle[] BordesHitbox { get; private set; }

        public Escenario(Texture2D textura)
        {
            TexturaFondo = textura;
            BordesHitbox = new Rectangle[4];

            int margen = 200;
            int grosor = 20;
            int anchoInterior = textura.Width - (margen * 2);
            int altoInterior = textura.Height - (margen * 2);

            BordesHitbox[0] = new Rectangle(margen, margen, anchoInterior, grosor);
            BordesHitbox[1] = new Rectangle(margen, textura.Height - margen - grosor, anchoInterior, grosor);
            BordesHitbox[2] = new Rectangle(margen, margen, grosor, altoInterior);
            BordesHitbox[3] = new Rectangle(textura.Width - margen - grosor, margen, grosor, altoInterior);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TexturaFondo, Vector2.Zero, Color.White);
        }
    }
}