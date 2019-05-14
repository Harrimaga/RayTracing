using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template;

namespace INFOGR2019Tmpl8
{
    public class Camera
    {
        public Vector3 position, direction;
        public Vector3[] screen;
        public Vector3 center;
        private Surface s;

        public Camera(Vector3 position, Vector3 direction, Surface screen)
        {
            this.position = position;
            this.direction = direction;
            center = position + direction;
            this.s = screen;
            ChangeScreen();
        }

        public void ChangeScreen()
        {

            float scaleFactor = (float)s.width / (float)s.height;

            screen = new Vector3[3] {
                (center + new Vector3(-1, -1 / scaleFactor, 0)),
                (center + new Vector3( 1, -1 / scaleFactor, 0)),
                (center + new Vector3(-1,  1 / scaleFactor, 0))
            };
        }
    }
}
