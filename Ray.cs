using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOGR2019Tmpl8
{
    public class Ray
    {
        public Vector3 origin;
        public Vector3 direction;
        public float distance;

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        public Ray(Vector3 origin, Vector3 direction, float distance)
        {
            this.origin = origin;
            this.direction = direction;
            this.distance = distance;
        }
    }
}
