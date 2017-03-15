using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Models
{
    public class WavefrontObj
    {
        public string FileName;

        public Vector3D[] Vertices;

        public Vector3D[] Normals;

        public Triangle[] Triangles;

        private WavefrontObj()
        {
        }

        public static WavefrontObj Open(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path is empty", nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new ArgumentException($"File {path} does not exist", nameof(path));
            }

            IList<Vector3D> vertices = new List<Vector3D>();
            IList<Vector3D> normals = new List<Vector3D>();
            IList<Triangle> faces = new List<Triangle>();

            foreach (string line in File.ReadLines(path))
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                switch (line)
                {
                    case string _ when line.Substring(0, 2) == "vn":
                        {
                            string[] normalParts = line.Split(' ', ',');
                            double x = double.Parse(normalParts[1], CultureInfo.InvariantCulture);
                            double y = double.Parse(normalParts[2], CultureInfo.InvariantCulture);
                            double z = -double.Parse(normalParts[3], CultureInfo.InvariantCulture);
                            var normal = new Vector3D(x, y, z);

                            normals.Add(normal);

                            break;
                        }
                    case string _ when line[0] == 'v':
                        {
                            string[] vectorParts = line.Split(' ');
                            double x = double.Parse(vectorParts[1], CultureInfo.InvariantCulture);
                            double y = double.Parse(vectorParts[2], CultureInfo.InvariantCulture);
                            double z = -double.Parse(vectorParts[3], CultureInfo.InvariantCulture);
                            var vertex = new Vector3D(x, y, z);

                            vertices.Add(vertex);

                            break;
                        }
                    case string _ when line[0] == 'f':
                        {
                            string[] vectorDefinitions = line.Split(' ');

                            var faceVertices = new FaceVertex[vectorDefinitions.Length - 1];

                            for (int i = 0; i < vectorDefinitions.Length - 1; i++)
                            {
                                var vectorDefinition = vectorDefinitions[i + 1].Split('/');

                                int vertexIndex = int.Parse(vectorDefinition[0], CultureInfo.InvariantCulture);
                                Vector3D vertex = vertices[vertexIndex - 1];

                                //todo textures
                                //int textureIndex = int.Parse(vectorDefinition[1], CultureInfo.InvariantCulture);
                                Texture texture = null;

                                int normalIndex = int.Parse(vectorDefinition[2], CultureInfo.InvariantCulture);
                                Vector3D normal = normals[(normalIndex - 1) / 3];

                                var fv = new FaceVertex
                                {
                                    Normal = normal,
                                    Texture = texture,
                                    Vertex = vertex
                                };
                                faceVertices[i] = fv;
                            }

                            var face = new Triangle
                            {
                                Vertex1 = faceVertices[0],
                                Vertex2 = faceVertices[1],
                                Vertex3 = faceVertices[2]
                            };
                            faces.Add(face);

                            break;
                        }
                }
            }

            var wavefront = new WavefrontObj
            {
                FileName = path,
                Triangles = faces.ToArray(),
                Normals = normals.ToArray(),
                Vertices = vertices.ToArray(),
            };

            return wavefront;
        }
    }
}
