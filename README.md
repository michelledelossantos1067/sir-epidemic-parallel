# Simulación Paralela de Epidemias - Modelo SIR

Simulador epidemiológico basado en el modelo SIR (Susceptible-Infectado-Recuperado) implementado en C# .NET 8.0 con versiones secuencial y paralela para análisis de desempeño.

## Descripción

Este proyecto implementa un modelo SIR sobre una grilla bidimensional de 1000×1000 celdas (1 millón de individuos) para simular la propagación de epidemias. Incluye implementaciones secuencial y paralela con análisis de escalabilidad fuerte.

## Características

- **Modelo SIR completo** con estados: Susceptible, Infectado, Recuperado, Fallecido
- **Versión secuencial** optimizada como línea base
- **Versión paralela** con descomposición de dominio (1-8 núcleos)
- **Visualizaciones** con gráficas de evolución temporal y animaciones espaciales
- **Análisis de desempeño** con métricas de speedup y eficiencia
- **Exportación de datos** en formato CSV para análisis posterior

## Estructura del Proyecto

```
sir-parallel-sim/
├── docs/                           # Documentación del proyecto
├── parallel/                       # Implementación paralela
│   ├── BlockProcessor.cs          # Procesamiento de bloques
│   ├── GridSimulatorParallel.cs   # Orquestador paralelo
│   ├── SIRModelParallel.cs        # Modelo SIR paralelo
│   ├── Statistics.cs              # Recolección de estadísticas
│   ├── Visualizer.cs              # Generación de visualizaciones
│   └── Program.cs                 # Punto de entrada
├── sequential/                     # Implementación secuencial
│   ├── GridSimulator.cs           # Orquestador secuencial
│   ├── SIRModel.cs                # Modelo SIR secuencial
│   ├── Statistics.cs              # Recolección de estadísticas
│   ├── Visualizer.cs              # Generación de visualizaciones
│   └── Program.cs                 # Punto de entrada
├── scripts/                        # Scripts de automatización
│   ├── generate_visuals.sh        # Genera visualizaciones
│   ├── run_all.sh                 # Pipeline completo
│   ├── run_benchmark.sh           # Análisis de escalabilidad
│   ├── run_parallel.sh            # Ejecuta versión paralela
│   └── run_sequential.sh          # Ejecuta versión secuencial
├── visualization/                  # Scripts Python para gráficas
│   ├── compare_versions.py        # Compara ambas versiones
│   ├── create_animation.py        # Genera animaciones GIF
│   ├── plot_results.py            # Genera gráficas de evolución
│   └── requirements.txt           # Dependencias Python
└── results/                        # Resultados de simulaciones
    ├── animation_parallel.gif
    ├── animation_sequential.gif
    ├── comparison.png
    ├── comparison_report.txt
    ├── parallel_evolution.png
    ├── parallel_stats.csv
    ├── parallel_time.txt
    ├── scaling_analysis.png
    ├── scaling_results.csv
    ├── sequential_evolution.png
    ├── sequential_stats.csv
    ├── sequential_time.txt
    └── side_by_side.gif
```

## Requisitos

- .NET 8.0 SDK
- Python 3.8+ (para visualizaciones)
- Bash (para scripts de automatización)

### Dependencias Python

```bash
pip install -r visualization/requirements.txt
```

Paquetes requeridos: `pandas`, `matplotlib`, `numpy`, `Pillow`

## Instalación

```bash
# Clonar el repositorio
git clone https://github.com/michelledelossantos1067/sir-epidemic-parallel.git
cd sir-parallel-sim

# Compilar ambas versiones
dotnet build sequential/sequential.csproj -c Release
dotnet build parallel/parallel.csproj -c Release
```

## Uso

### Ejecución Individual

**Versión secuencial:**
```bash
cd scripts
./run_sequential.sh
```

**Versión paralela:**
```bash
cd scripts
./run_parallel.sh
# Ejemplo: ./run_parallel.sh 8
```

### Pipeline Completo

Ejecuta ambas versiones, análisis de escalabilidad y genera todas las visualizaciones:

```bash
cd scripts
./run_all.sh
```

### Análisis de Escalabilidad

Ejecuta experimentos con 1, 2, 4 y 8 núcleos:

```bash
cd scripts
./run_benchmark.sh
```

### Generar Visualizaciones

```bash
cd scripts
./generate_visuals.sh
```

## 📊 Resultados

Los resultados se guardan en el directorio `results/`:

### Archivos de Datos

- `sequential_stats.csv` / `parallel_stats.csv`: Estadísticas diarias (Susceptibles, Infectados, Recuperados, Fallecidos, R₀)
- `scaling_results.csv`: Resultados de escalabilidad (Cores, Tiempo, Speedup, Eficiencia)
- `sequential_time.txt` / `parallel_time.txt`: Tiempos de ejecución

### Visualizaciones

- `sequential_evolution.png` / `parallel_evolution.png`: Evolución temporal de compartimentos
- `comparison.png`: Comparación lado a lado de ambas versiones
- `scaling_analysis.png`: Gráficas de speedup y eficiencia
- `animation_sequential.gif` / `animation_parallel.gif`: Animaciones de propagación espacial
- `side_by_side.gif`: Comparación animada de ambas versiones
- `comparison_report.txt`: Reporte textual de diferencias

## Parámetros del Modelo

| Parámetro | Valor | Descripción |
|-----------|-------|-------------|
| Población | 1,000,000 | Grilla 1000×1000 |
| Infectados iniciales | 10 | Casos semilla |
| Tasa de infección (β) | 0.30 | Probabilidad de contagio por contacto |
| Tasa de recuperación (γ) | 0.10 | Probabilidad diaria de recuperación |
| Tasa de mortalidad (μ) | 0.01 | Probabilidad diaria de muerte |
| Duración | 365 días | Periodo de simulación |

## Desempeño

Tiempos promedio de ejecución en procesador multi-core:

| Núcleos | Tiempo | Speedup | Eficiencia |
|---------|--------|---------|-----------|
| 1 | 2.90s | 1.00× | 100.0% |
| 2 | 1.97s | 1.47× | 73.7% |
| 4 | 1.78s | 1.63× | 40.7% |
| 8 | 1.40s | 2.07× | 25.8% |

## Tecnologías

- **C# .NET 8.0**: Implementación del modelo y simulación
- **Parallel Extensions (TPL)**: Paralelización con Task Parallel Library
- **Python**: Generación de visualizaciones
- **Matplotlib**: Gráficas de evolución temporal
- **Pillow**: Creación de animaciones GIF
- **Bash**: Scripts de automatización

## 📝 Formato de Datos CSV

### statistics.csv
```csv
Day,Susceptible,Infected,Recovered,Dead,R0
1,999961,39,0,0,4.1000
2,999894,103,3,0,11.6000
...
```

### scaling_results.csv
```csv
Cores,Time,Speedup,Efficiency
1,2.9000,1.0000,1.0000
2,1.9671,1.4743,0.7371
...
```

## Licencia

Este proyecto es de código abierto y está disponible para fines educativos.

## Autor

Dianny Michele De los santos De los santos - 2024-0213
## Referencias

1. Keeling & Rohani (2008). Modeling Infectious Diseases
2. Kermack & McKendrick (1927). Mathematical theory of epidemics
3. Amdahl (1967). Single processor approach to computing
