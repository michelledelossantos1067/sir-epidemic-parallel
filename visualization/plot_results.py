import pandas as pd
import matplotlib.pyplot as plt
import sys
import os

def plot_epidemic_evolution(csv_file, output_file, title):
    df = pd.read_csv(csv_file)
    
    fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(12, 10))
    
    ax1.plot(df['Day'], df['Susceptible'], 'b-', label='Susceptible', linewidth=2)
    ax1.plot(df['Day'], df['Infected'], 'r-', label='Infectado', linewidth=2)
    ax1.plot(df['Day'], df['Recovered'], 'g-', label='Recuperado', linewidth=2)
    ax1.plot(df['Day'], df['Dead'], 'k-', label='Fallecido', linewidth=2)
    ax1.set_xlabel('Días', fontsize=12)
    ax1.set_ylabel('Población', fontsize=12)
    ax1.set_title(title, fontsize=14, fontweight='bold')
    ax1.legend(fontsize=10)
    ax1.grid(True, alpha=0.3)
    
    final_stats = df.iloc[-1]
    categories = ['Susceptible', 'Infectado', 'Recuperado', 'Fallecido']
    values = [final_stats['Susceptible'], final_stats['Infected'], 
              final_stats['Recovered'], final_stats['Dead']]
    colors = ['blue', 'red', 'green', 'black']
    
    ax2.bar(categories, values, color=colors, alpha=0.7)
    ax2.set_ylabel('Población', fontsize=12)
    ax2.set_title('Estado Final de la Población', fontsize=14, fontweight='bold')
    ax2.grid(True, alpha=0.3, axis='y')
    
    for i, v in enumerate(values):
        ax2.text(i, v + max(values)*0.01, f'{int(v):,}', 
                ha='center', va='bottom', fontweight='bold')
    
    plt.tight_layout()
    plt.savefig(output_file, dpi=300, bbox_inches='tight')
    print(f"Grafica guardada: {output_file}")

def plot_comparison(seq_csv, par_csv, output_file):
    df_seq = pd.read_csv(seq_csv)
    df_par = pd.read_csv(par_csv)
    
    fig, axes = plt.subplots(2, 2, figsize=(16, 12))
    
    ax1 = axes[0, 0]
    ax1.plot(df_seq['Day'], df_seq['Infected'], 'r-', label='Secuencial', linewidth=2)
    ax1.plot(df_par['Day'], df_par['Infected'], 'b--', label='Paralela', linewidth=2)
    ax1.set_xlabel('Días')
    ax1.set_ylabel('Infectados')
    ax1.set_title('Comparación de Infectados', fontweight='bold')
    ax1.legend()
    ax1.grid(True, alpha=0.3)
    
    ax2 = axes[0, 1]
    ax2.plot(df_seq['Day'], df_seq['R0'], 'r-', label='Secuencial', linewidth=2)
    ax2.plot(df_par['Day'], df_par['R0'], 'b--', label='Paralela', linewidth=2)
    ax2.set_xlabel('Días')
    ax2.set_ylabel('R₀')
    ax2.set_title('Comparación de R₀', fontweight='bold')
    ax2.legend()
    ax2.grid(True, alpha=0.3)
    
    ax3 = axes[1, 0]
    for col, color, label in [('Susceptible', 'blue', 'Susceptible'),
                               ('Infected', 'red', 'Infectado'),
                               ('Recovered', 'green', 'Recuperado'),
                               ('Dead', 'black', 'Fallecido')]:
        ax3.plot(df_seq['Day'], df_seq[col], color=color, label=label, linewidth=2)
    ax3.set_xlabel('Días')
    ax3.set_ylabel('Población')
    ax3.set_title('Evolución - Secuencial', fontweight='bold')
    ax3.legend()
    ax3.grid(True, alpha=0.3)
    
    ax4 = axes[1, 1]
    for col, color, label in [('Susceptible', 'blue', 'Susceptible'),
                               ('Infected', 'red', 'Infectado'),
                               ('Recovered', 'green', 'Recuperado'),
                               ('Dead', 'black', 'Fallecido')]:
        ax4.plot(df_par['Day'], df_par[col], color=color, label=label, linewidth=2)
    ax4.set_xlabel('Días')
    ax4.set_ylabel('Población')
    ax4.set_title('Evolución - Paralela', fontweight='bold')
    ax4.legend()
    ax4.grid(True, alpha=0.3)
    
    plt.tight_layout()
    plt.savefig(output_file, dpi=300, bbox_inches='tight')
    print(f"Comparación guardada: {output_file}")

def plot_scaling(csv_file, output_file):
    df = pd.read_csv(csv_file)
    
    fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(14, 6))
    
    cores = df['Cores'].values
    speedup = df['Speedup'].values
    
    ax1.plot(cores, speedup, 'bo-', linewidth=2, markersize=10, label='Speedup real')
    ax1.plot(cores, cores, 'r--', linewidth=2, label='Speedup ideal (lineal)')
    ax1.set_xlabel('Número de Cores', fontsize=12)
    ax1.set_ylabel('Speedup', fontsize=12)
    ax1.set_title('Strong Scaling - Speedup', fontsize=14, fontweight='bold')
    ax1.legend(fontsize=10)
    ax1.grid(True, alpha=0.3)
    ax1.set_xticks(cores)
    
    times = df['Time'].values
    ax2.plot(cores, times, 'go-', linewidth=2, markersize=10)
    ax2.set_xlabel('Número de Cores', fontsize=12)
    ax2.set_ylabel('Tiempo (segundos)', fontsize=12)
    ax2.set_title('Tiempo de Ejecución vs Cores', fontsize=14, fontweight='bold')
    ax2.grid(True, alpha=0.3)
    ax2.set_xticks(cores)
    
    for i, (c, t) in enumerate(zip(cores, times)):
        ax2.text(c, t, f'{t:.2f}s', ha='center', va='bottom', fontweight='bold')
    
    plt.tight_layout()
    plt.savefig(output_file, dpi=300, bbox_inches='tight')
    print(f"Analisis de scaling guardado: {output_file}")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Uso: python plot_results.py [sequential|parallel|comparison|scaling]")
        sys.exit(1)
    
    mode = sys.argv[1]
    
    if mode == "sequential":
        plot_epidemic_evolution(
            "../sequential/output/statistics.csv",
            "../results/sequential_evolution.png",
            "Evolución de la Epidemia - Secuencial"
        )
    
    elif mode == "parallel":
        plot_epidemic_evolution(
            "../parallel/output/statistics.csv",
            "../results/parallel_evolution.png",
            "Evolución de la Epidemia - Paralela"
        )
    
    elif mode == "comparison":
        plot_comparison(
            "../sequential/output/statistics.csv",
            "../parallel/output/statistics.csv",
            "../results/comparison.png"
        )
    
    elif mode == "scaling":
        plot_scaling(
            "../results/scaling_results.csv",
            "../results/scaling_analysis.png"
        )
    
    else:
        print("Modo inválido. Use: sequential, parallel, comparison, o scaling")