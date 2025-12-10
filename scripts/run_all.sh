#!/bin/bash

echo "SIMULACION MONTE-CARLO SIR - EJECUCION COMPLETA"
echo ""

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

mkdir -p ../results

echo "1. Ejecutando simulacion secuencial..."
./run_sequential.sh
if [ $? -ne 0 ]; then
    echo "Error en simulacion secuencial"
    exit 1
fi

echo ""
echo "2. Ejecutando simulacion paralela..."
./run_parallel.sh
if [ $? -ne 0 ]; then
    echo "Error en simulacion paralela"
    exit 1
fi

echo ""
echo "3. Ejecutando benchmark de scaling..."
./run_benchmark.sh
if [ $? -ne 0 ]; then
    echo "Error en benchmark"
    exit 1
fi

echo ""
echo "4. Generando visualizaciones..."
./generate_visuals.sh
if [ $? -ne 0 ]; then
    echo "Error generando visualizaciones"
    exit 1
fi

echo ""
echo "EJECUCION COMPLETADA"
echo ""
echo "Todos los archivos generados en results/:"
echo ""
ls -lh ../results/ | tail -n +2 | awk '{printf "  %-40s %10s\n", $9, $5}'
echo ""
echo ""
echo "Para ver el reporte de comparacion:"
echo "  cat ../results/comparison_report.txt"
echo ""
echo "Para ver los resultados de scaling:"
echo "  cat ../results/scaling_results.csv"
echo ""
