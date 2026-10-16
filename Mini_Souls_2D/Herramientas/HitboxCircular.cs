using Microsoft.Xna.Framework;

using Mini_Souls_2D.enemigos;
using Mini_Souls_2D.personaje;
using Mini_Souls_2D.principal;

namespace Mini_Souls_2D.herramientas
{
    public struct HitboxCircular
    {
        public Vector2 Centro;
        public float Radio;

        public HitboxCircular(Vector2 centro, float radio)
        {
            Centro = centro;
            Radio = radio;
        }

        public bool IntersectaConRectangulo(Rectangle rect)
        {
            float puntoMasCercanoX = MathHelper.Clamp(Centro.X, rect.Left, rect.Right);
            float puntoMasCercanoY = MathHelper.Clamp(Centro.Y, rect.Top, rect.Bottom);
            float distanciaX = Centro.X - puntoMasCercanoX;
            float distanciaY = Centro.Y - puntoMasCercanoY;
            float distanciaAlCuadrado = (distanciaX * distanciaX) + (distanciaY * distanciaY);

            return distanciaAlCuadrado < (Radio * Radio);
        }
    }
}