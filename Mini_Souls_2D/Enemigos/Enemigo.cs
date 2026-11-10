using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Mini_Souls_2D.personaje;

namespace Mini_Souls_2D.Enemigos
{
    public abstract class Enemigo
    {
        public Vector2 Posicion;
        public Texture2D movimiento;
        public Texture2D ataque;
        public int vida;
        public int daño;

        public Boolean EstaVivo;

        public abstract void atacar();

        public abstract void RecibirDaño(int cantidad);

        public abstract void aturdir();

    }
}
