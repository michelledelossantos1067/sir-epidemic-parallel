#!/bin/bash

echo "GENERANDO VISUALIZACIONES"

cd ../visualization

if ! command -v python3 &> /dev/null; then
    echo "Error: Python3 no esta instalado"
    exit 1
fi

echo "Verificando dependencias de Python..."
pip3 install -q -r requirements.txt

if [ $? -ne 0 ]; then
    echo "Error instalando dependencias"
    exit 1
fi

echo ""
echo "1. Generando graficas de evolucion secuencial..."
python3 plot_results.py sequential

echo ""
echo "2. Generando graficas de evolucion paralela..."
python3 plot_results.py parallel

echo ""
echo "3. Generando graficas de comparacion..."
python3 plot_results.py comparison

echo ""
echo "4. Generando graficas de scaling..."
if [ -f "../results/scaling_results.csv" ]; then
    python3 plot_results.py scaling
else
    echo "Advertencia: scaling_results.csv no encontrado. Ejecuta run_benchmark.sh primero."
fi

echo ""
echo "5. Generando animaciones..."
python3 create_animation.py

echo ""
echo "6. Comparando versiones..."
if [ -f "../sequential/output/statistics.csv" ] && [ -f "../parallel/output/statistics.csv" ]; then
    python3 compare_versions.py \
        ../sequential/output/statistics.csv \
        ../parallel/output/statistics.csv \
        ../sequential/output/execution_time.txt \
        ../parallel/output/execution_time.txt
else
    echo "Advertencia: Archivos de estadisticas no encontrados."
fi

echo "VISUALIZACIONES COMPLETADAS"
echo "Archivos generados en results/:"
ls -lh ../results/*.png ../results/*.gif 2>/dev/null | awk '{print "  - " $9}'
echo ""