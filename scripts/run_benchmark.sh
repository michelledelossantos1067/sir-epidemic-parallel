#!/bin/bash

echo "EJECUTANDO BENCHMARK DE STRONG SCALING"
echo ""

cd ../parallel

echo "Compilando version paralela..."
dotnet build -c Release > /dev/null 2>&1

if [ $? -ne 0 ]; then
    echo "Error en compilacion"
    exit 1
fi

mkdir -p ../results

echo "Cores,Time,Speedup,Efficiency" > ../results/scaling_results.csv

CORE_COUNTS=(1 2 4 8)
BASE_TIME=0

echo ""
echo "Ejecutando experimentos de scaling..."
echo ""

for CORES in "${CORE_COUNTS[@]}"; do
    echo "Ejecutando con $CORES core(s)..."
    
    dotnet run -c Release $CORES > /dev/null 2>&1
    
    if [ $? -ne 0 ]; then
        echo "Error en ejecucion con $CORES cores"
        continue
    fi
    
    TIME=$(head -1 output/execution_time.txt)
    
    echo "Tiempo: $TIME segundos"
    
    if [ $CORES -eq 1 ]; then
        BASE_TIME=$TIME
        SPEEDUP="1.0000"
        EFFICIENCY="1.0000"
    else
        SPEEDUP=$(python3 -c "print(f'{$BASE_TIME / $TIME:.4f}')")
        EFFICIENCY=$(python3 -c "print(f'{($BASE_TIME / $TIME) / $CORES:.4f}')")
    fi
    
    echo "$CORES,$TIME,$SPEEDUP,$EFFICIENCY" >> ../results/scaling_results.csv
    
    EFFICIENCY_PCT=$(python3 -c "print(f'{float($EFFICIENCY) * 100:.1f}')")
    echo "Speedup: ${SPEEDUP}x"
    echo "Eficiencia: ${EFFICIENCY_PCT}%"
    echo ""
done

echo "RESULTADOS DE SCALING"
echo ""

column -t -s',' ../results/scaling_results.csv

echo ""
echo "Resultados guardados en: results/scaling_results.csv"
echo ""