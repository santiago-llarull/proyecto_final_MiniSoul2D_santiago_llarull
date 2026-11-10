using Microsoft.Xna.Framework;

using Mini_Souls_2D.Enemigos;
using Mini_Souls_2D.personaje;
using Mini_Souls_2D.principal;

namespace Mini_Souls_2D.herramientas
{
    public class Camara2D
    {
        public Matrix Transformacion { get; private set; }

        public void Actualizar(Vector2 posicionObjetivo, int anchoPantalla, int altoPantalla)
        {
            Transformacion =
                Matrix.CreateTranslation(new Vector3(-posicionObjetivo.X, -posicionObjetivo.Y, 0)) *
                Matrix.CreateScale(new Vector3(1f, 1f, 1)) *
                Matrix.CreateTranslation(new Vector3(anchoPantalla * 0.5f, altoPantalla * 0.5f, 0));
        }
    }
}