#!/bin/bash

echo "         EJECUTANDO SIMULACION SECUENCIAL                    "
echo ""

cd ../sequential

echo "Compilando version secuencial..."
dotnet build -c Release

if [ $? -ne 0 ]; then
    echo "Error en compilacion"
    exit 1
fi

echo ""
echo "Ejecutando simulacion..."
echo ""

dotnet run -c Release

if [ $? -ne 0 ]; then
    echo "Error en ejecucion"
    exit 1
fi

echo ""
echo "Copiando resultados a ../results/..."

mkdir -p ../results
cp output/statistics.csv ../results/sequential_stats.csv
cp output/execution_time.txt ../results/sequential_time.txt

echo ""
echo "Simulacion secuencial completada exitosamente"
echo "Archivos generados:"
echo "  - sequential/output/statistics.csv"
echo "  - sequential/output/execution_time.txt"
echo "  - sequential/output/grid_snapshots/"
echo ""