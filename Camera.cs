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
        Vector3 position, direction;
        Vector3[] screen;
        Vector3 center;

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
