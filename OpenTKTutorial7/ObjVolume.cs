using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.IO;

namespace OpenTKTutorial7
{
    class ObjVolume : Volume
    {
        Vector3[] vertices;
        Vector3[] colors;
        Vector2[] texturecoords;

        List<Tuple<int, int, int>> faces = new List<Tuple<int, int, int>>();

        public override int VertCount
        {
            get
            {
                return vertices.Length;
            }
        }

        public override int IndiceCount
        {
            get
            {
                return faces.Count * 3;
            }
        }

        public override int ColorDataCount
        {
            get
            {
                return colors.Length;
            }
        }

        public override void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.Scale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
        }

        public override Vector3[] GetColorData()
        {
            return colors;
        }

        public override int[] GetIndices(int offset = 0)
        {
            List<int> temp = new List<int>();
            foreach (var face in faces)
            {
                temp.Add(face.Item1 + offset);
                temp.Add(face.Item2 + offset);
                temp.Add(face.Item3 + offset);
            }

            return temp.ToArray();
        }

        public override Vector2[] GetTextureCoords()
        {
            return texturecoords;
        }

        public override Vector3[] GetVerts()
        {
            return vertices;
        }

        public static ObjVolume LoadFromFile(string filename)
        {
            ObjVolume obj = new ObjVolume();
            try
            {
                using(StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
                {
                    obj = LoadFromString(sr.ReadToEnd());
                }
            }
            catch(FileNotFoundException ex)
            {
                Console.WriteLine("File not found: {0}", filename);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Loading file: {0}", filename);
            }
            return obj;
        }

        public static ObjVolume LoadFromString(string obj)
        {
            // Seperate lines from the file
            List<String> lines = new List<string>(obj.Split('\n'));

            // Lists to hold model data
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> texs = new List<Vector2>();
            List<Tuple<int, int, int>> faces = new List<Tuple<int, int, int>>();

            // Read file line by line
            foreach (String line in lines)
            {
                if (line.StartsWith("v ")) // Vertex definition
                {
                    // Cut off beginning of line
                    String temp = line.Substring(2);

                    Vector3 vec = new Vector3();

                    if (temp.Count((char c) => c == ' ') == 2) // Check if there's enough elements for a vertex
                    {
                        String[] vertparts = temp.Split(' ');

                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(vertparts[0], out vec.X);
                        success |= float.TryParse(vertparts[1], out vec.Y);
                        success |= float.TryParse(vertparts[2], out vec.Z);

                        // Dummy color/texture coordinates for now
                        colors.Add(new Vector3((float)Math.Sin(vec.Z), (float)Math.Sin(vec.Z), (float)Math.Sin(vec.Z)));
                        texs.Add(new Vector2((float)Math.Sin(vec.Z), (float)Math.Sin(vec.Z)));

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing vertex: {0}", line);
                        }
                    }

                    verts.Add(vec);
                }
                else if (line.StartsWith("f ")) // Face definition
                {
                    // Cut off beginning of line
                    String temp = line.Substring(2);

                    Tuple<int, int, int> face = new Tuple<int, int, int>(0, 0, 0);

                    if (temp.Count((char c) => c == ' ') == 2) // Check if there's enough elements for a face
                    {
                        String[] faceparts = temp.Split(' ');

                        int i1, i2, i3;

                        // Attempt to parse each part of the face
                        bool success = int.TryParse(faceparts[0], out i1);
                        success |= int.TryParse(faceparts[1], out i2);
                        success |= int.TryParse(faceparts[2], out i3);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing face: {0}", line);
                        }
                        else
                        {
                            // Decrement to get zero-based vertex numbers
                            face = new Tuple<int, int, int>(i1 - 1, i2 - 1, i3 - 1);
                            faces.Add(face);
                        }
                    }
                }
            }

            // Create the ObjVolume
            ObjVolume vol = new ObjVolume();
            vol.vertices = verts.ToArray();
            vol.faces = new List<Tuple<int, int, int>>(faces);
            vol.colors = colors.ToArray();
            vol.texturecoords = texs.ToArray();

            return vol;
        }
    }
}
