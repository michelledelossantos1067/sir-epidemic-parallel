#!/bin/bash

CORES=${1:-0}

echo "EJECUTANDO SIMULACION PARALELA"
echo ""

cd ../parallel

echo "Compilando version paralela..."
dotnet build -c Release

if [ $? -ne 0 ]; then
    echo "Error en compilacion"
    exit 1
fi

echo ""

if [ $CORES -eq 0 ]; then
    echo "Ejecutando con todos los cores disponibles..."
    dotnet run -c Release
else
    echo "Ejecutando con $CORES core(s)..."
    dotnet run -c Release $CORES
fi

if [ $? -ne 0 ]; then
    echo "Error en ejecucion"
    exit 1
fi

echo ""
echo "Copiando resultados a ../results/..."

mkdir -p ../results
cp output/statistics.csv ../results/parallel_stats.csv
cp output/execution_time.txt ../results/parallel_time.txt

echo ""
echo "Simulacion paralela completada exitosamente"
echo "Archivos generados:"
echo "  - parallel/output/statistics.csv"
echo "  - parallel/output/execution_time.txt"
echo "  - parallel/output/grid_snapshots/"
echo ""