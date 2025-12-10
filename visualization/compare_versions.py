import pandas as pd
import numpy as np
import sys

def compare_statistics(seq_file, par_file):
    df_seq = pd.read_csv(seq_file)
    df_par = pd.read_csv(par_file)
    
    print("=" * 70)
    print("COMPARACION DE RESULTADOS: SECUENCIAL VS PARALELA")
    print("=" * 70)
    print()
    
    print("Verificacion de identidad de resultados:")
    print("-" * 70)
    
    days_match = len(df_seq) == len(df_par)
    print(f"Numero de dias simulados: {'IGUAL' if days_match else 'DIFERENTE'}")
    print(f"  Secuencial: {len(df_seq)} dias")
    print(f"  Paralela:   {len(df_par)} dias")
    print()
    
    if days_match:
        min_days = min(len(df_seq), len(df_par))
        
        for col in ['Susceptible', 'Infected', 'Recovered', 'Dead']:
            diff = np.abs(df_seq[col][:min_days] - df_par[col][:min_days])
            max_diff = diff.max()
            avg_diff = diff.mean()
            
            status = "IDENTICO" if max_diff == 0 else f"Diff max: {max_diff}"
            print(f"{col:12} - {status}")
        
        print()
        r0_diff = np.abs(df_seq['R0'][:min_days] - df_par['R0'][:min_days])
        print(f"R0           - Diff max: {r0_diff.max():.6f}, Diff avg: {r0_diff.mean():.6f}")
    
    print()
    print("Estado final:")
    print("-" * 70)
    
    final_seq = df_seq.iloc[-1]
    final_par = df_par.iloc[-1]
    
    print(f"{'Estado':<15} {'Secuencial':>12} {'Paralela':>12} {'Diferencia':>12}")
    print("-" * 70)
    
    for col in ['Susceptible', 'Infected', 'Recovered', 'Dead']:
        seq_val = final_seq[col]
        par_val = final_par[col]
        diff = abs(seq_val - par_val)
        print(f"{col:<15} {seq_val:>12.0f} {par_val:>12.0f} {diff:>12.0f}")
    
    print()
    print(f"R0 final:       {final_seq['R0']:>12.4f} {final_par['R0']:>12.4f} "
          f"{abs(final_seq['R0'] - final_par['R0']):>12.6f}")
    
    print()
    print("=" * 70)
    
    total_diff = sum(abs(final_seq[col] - final_par[col]) 
                    for col in ['Susceptible', 'Infected', 'Recovered', 'Dead'])
    
    if total_diff == 0:
        print("VALIDACION: ✓ Resultados IDENTICOS")
    elif total_diff < 100:
        print("VALIDACION: ✓ Resultados practicamente IDENTICOS (diferencias minimas)")
    else:
        print(f"VALIDACION: ✗ Diferencias significativas detectadas (total: {total_diff})")
    
    print("=" * 70)
    
    return total_diff == 0

def compare_execution_times(seq_time_file, par_time_file):
    with open(seq_time_file, 'r') as f:
        seq_time = float(f.read().strip())
    
    with open(par_time_file, 'r') as f:
        lines = f.readlines()
        par_time = float(lines[0].strip())
        num_cores = int(lines[1].strip()) if len(lines) > 1 else 1
    
    speedup = seq_time / par_time
    efficiency = speedup / num_cores
    
    print()
    print("=" * 70)
    print("ANALISIS DE DESEMPEÑO")
    print("=" * 70)
    print()
    print(f"Tiempo secuencial:        {seq_time:>10.2f} segundos")
    print(f"Tiempo paralelo:          {par_time:>10.2f} segundos")
    print(f"Cores utilizados:         {num_cores:>10}")
    print()
    print(f"Speedup:                  {speedup:>10.2f}x")
    print(f"Speedup ideal:            {num_cores:>10.2f}x")
    print(f"Eficiencia:               {efficiency:>10.1%}")
    print()
    print("=" * 70)
    
    return speedup, efficiency

def generate_summary_report(seq_stats, par_stats, seq_time, par_time, output_file):
    df_seq = pd.read_csv(seq_stats)
    df_par = pd.read_csv(par_stats)
    
    with open(seq_time, 'r') as f:
        time_seq = float(f.read().strip())
    
    with open(par_time, 'r') as f:
        lines = f.readlines()
        time_par = float(lines[0].strip())
        cores = int(lines[1].strip()) if len(lines) > 1 else 1
    
    speedup = time_seq / time_par
    efficiency = speedup / cores
    
    final_seq = df_seq.iloc[-1]
    final_par = df_par.iloc[-1]
    
    with open(output_file, 'w') as f:
        f.write("REPORTE DE COMPARACION - SIMULACION SIR\n")
        f.write("=" * 70 + "\n\n")
        
        f.write("1. CONFIGURACION\n")
        f.write("-" * 70 + "\n")
        f.write(f"Dias simulados:     {len(df_seq)}\n")
        f.write(f"Cores utilizados:   {cores}\n\n")
        
        f.write("2. RESULTADOS FINALES\n")
        f.write("-" * 70 + "\n")
        f.write(f"Susceptibles:       {final_par['Susceptible']:.0f}\n")
        f.write(f"Infectados:         {final_par['Infected']:.0f}\n")
        f.write(f"Recuperados:        {final_par['Recovered']:.0f}\n")
        f.write(f"Fallecidos:         {final_par['Dead']:.0f}\n")
        f.write(f"R0 final:           {final_par['R0']:.4f}\n\n")
        
        f.write("3. DESEMPEÑO\n")
        f.write("-" * 70 + "\n")
        f.write(f"Tiempo secuencial:  {time_seq:.2f} segundos\n")
        f.write(f"Tiempo paralelo:    {time_par:.2f} segundos\n")
        f.write(f"Speedup:            {speedup:.2f}x\n")
        f.write(f"Eficiencia:         {efficiency:.1%}\n\n")
        
        f.write("4. VALIDACION\n")
        f.write("-" * 70 + "\n")
        total_diff = sum(abs(final_seq[col] - final_par[col]) 
                        for col in ['Susceptible', 'Infected', 'Recovered', 'Dead'])
        
        if total_diff == 0:
            f.write("Estado: VALIDADO - Resultados identicos\n")
        else:
            f.write(f"Estado: Diferencia total = {total_diff}\n")
        
        f.write("\n" + "=" * 70 + "\n")
    
    print(f"\nReporte guardado en: {output_file}")

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Uso: python compare_versions.py <seq_stats.csv> <par_stats.csv> [seq_time.txt] [par_time.txt]")
        sys.exit(1)
    
    seq_stats = sys.argv[1]
    par_stats = sys.argv[2]
    
    compare_statistics(seq_stats, par_stats)
    
    if len(sys.argv) >= 5:
        seq_time = sys.argv[3]
        par_time = sys.argv[4]
        compare_execution_times(seq_time, par_time)
        
        generate_summary_report(seq_stats, par_stats, seq_time, par_time,
                              "../results/comparison_report.txt")
    
    print("\nComparacion completada.")