using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using ConwayGameOfLifeCore;
using System;

namespace ConwayGameOfLifeCoreSilk
{
    public class Program
    {
        private static LifeSimulation _sim;
        private static IWindow _window;
        private static GL _gl;

        private const float CellSize = 0.05f;
        private static uint _vao, _vbo;
        private static uint _shaderProgram;

        public static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "Conway's Game of Life - Silk.NET";

            _window = Window.Create(options);
            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.Run();
        }

        private static void OnLoad()
        {
            _gl = GL.GetApi(_window);

            _sim = new LifeSimulation(80, 40);
            _sim.Randomize();

            SetupGraphics();
        }

        private static unsafe void SetupGraphics()
        {
            // Define a quad as two triangles
            float[] vertices = {
                0f, 0f,
                CellSize, 0f,
                CellSize, CellSize,

                0f, 0f,
                CellSize, CellSize,
                0f, CellSize
            };

            // Generate VAO and VBO
            _vao = _gl.GenVertexArray();
            _vbo = _gl.GenBuffer();

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            fixed (void* v = &vertices[0])
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(uint)), v, BufferUsageARB.StaticDraw); //Setting buffer data.
            }
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            _gl.EnableVertexAttribArray(0);

            // Create shaders
            string vertexShaderSrc = @"
#version 330 core
layout (location = 0) in vec2 aPos;
uniform vec2 uOffset;
void main()
{
    gl_Position = vec4(aPos + uOffset, 0.0, 1.0);
}";

            string fragmentShaderSrc = @"
#version 330 core
out vec4 FragColor;
void main()
{
    FragColor = vec4(1.0, 1.0, 1.0, 1.0); // White cell
}";

            _shaderProgram = CreateShaderProgram(vertexShaderSrc, fragmentShaderSrc);
        }

        private static uint CreateShaderProgram(string vertexSource, string fragmentSource)
        {
            uint vertex = _gl.CreateShader(ShaderType.VertexShader);
            _gl.ShaderSource(vertex, vertexSource);
            _gl.CompileShader(vertex);
            CheckShaderCompile(vertex, "Vertex");

            uint fragment = _gl.CreateShader(ShaderType.FragmentShader);
            _gl.ShaderSource(fragment, fragmentSource);
            _gl.CompileShader(fragment);
            CheckShaderCompile(fragment, "Fragment");

            uint program = _gl.CreateProgram();
            _gl.AttachShader(program, vertex);
            _gl.AttachShader(program, fragment);
            _gl.LinkProgram(program);
            _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int status);
            if (status == 0)
                throw new Exception("Program linking failed.");

            _gl.DeleteShader(vertex);
            _gl.DeleteShader(fragment);

            return program;
        }

        private static void CheckShaderCompile(uint shader, string type)
        {
            _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);
            if (success == 0)
            {
                string log = _gl.GetShaderInfoLog(shader);
                throw new Exception($"{type} shader compilation failed: {log}");
            }
        }

        private static void OnUpdate(double deltaTime)
        {
            _sim.Update().GetAwaiter().GetResult();
        }

        private static void OnRender(double deltaTime)
        {
            _gl.ClearColor(0f, 0f, 0f, 1f);
            _gl.Clear((uint)ClearBufferMask.ColorBufferBit);

            DrawGrid();

            _window.SwapBuffers();
        }

        private static void DrawGrid()
        {
            _gl.UseProgram(_shaderProgram);
            _gl.BindVertexArray(_vao);

            int offsetLoc = _gl.GetUniformLocation(_shaderProgram, "uOffset");

            for (int y = 0; y < _sim.SizeY; y++)
            {
                for (int x = 0; x < _sim.SizeX; x++)
                {
                    if (!_sim[x, y]) continue;

                    float fx = x * CellSize - 1.0f;
                    float fy = y * CellSize - 1.0f;
                    _gl.Uniform2(offsetLoc, fx, fy);
                    _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
                }
            }
        }
    }
}