using System.Collections.Generic;
using SharpDX;

namespace Assemblies.Utilitys {
    public class Polygon //Credits to Detuks
    {
        public List<Vector2> Points = new List<Vector2>();

        public Polygon(List<Vector2> P) {
            Points = P;
        }

        public void Add(Vector2 vec) {
            Points.Add(vec);
        }

        public int Count() {
            return Points.Count;
        }

        public bool Contains(Vector2 point) {
            bool result = false;
            int j = Count() - 1;
            for (int i = 0; i < Count(); i++) {
                if (Points[i].Y < point.Y && Points[j].Y >= point.Y || Points[j].Y < point.Y && Points[i].Y >= point.Y) {
                    if (Points[i].X +
                        (point.Y - Points[i].Y)/(Points[j].Y - Points[i].Y)*(Points[j].X - Points[i].X) < point.X) {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
    }
}