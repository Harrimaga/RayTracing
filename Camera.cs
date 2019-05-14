using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOGR2019Tmpl8
{
    public class Camera
    {
        public Vector3 position, direction;
        public Vector3[] screen;
        public Vector3 center;

        public Camera(Vector3 position, Vector3 direction)
        {
            this.position = position;
            this.direction = direction;
            center = position + direction;
            ChangeScreen();
        }

        public void ChangeScreen()
        {
            screen = new Vector3[3] {
                (center + new Vector3(-1, -1, 0)),
                (center + new Vector3( 1, -1, 0)),
                (center + new Vector3(-1,  1, 0))
            };
        }
    }
}
