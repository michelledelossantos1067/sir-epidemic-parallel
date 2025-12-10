import numpy as np
import matplotlib.pyplot as plt
import matplotlib.animation as animation
from matplotlib.colors import ListedColormap
import os
import glob

def load_grid_snapshots(directory):
    files = sorted(glob.glob(os.path.join(directory, "grid_day_*.csv")))
    grids = []
    
    for file in files:
        grid = np.loadtxt(file, delimiter=',', dtype=int)
        grids.append(grid)
    
    return grids

def create_animation(grids, title, output_file, fps=10):
    fig, ax = plt.subplots(figsize=(10, 10))
    
    colors = ['#3b82f6', '#ef4444', '#22c55e', '#374151']
    cmap = ListedColormap(colors)
    
    ax.set_title(title, fontsize=16, fontweight='bold')
    ax.axis('off')
    
    im = ax.imshow(grids[0], cmap=cmap, vmin=0, vmax=3, interpolation='nearest')
    
    cbar = plt.colorbar(im, ax=ax, ticks=[0.375, 1.125, 1.875, 2.625])
    cbar.ax.set_yticklabels(['Susceptible', 'Infectado', 'Recuperado', 'Fallecido'])
    
    day_text = ax.text(0.02, 0.98, '', transform=ax.transAxes,
                      fontsize=14, verticalalignment='top', color='white',
                      bbox=dict(boxstyle='round', facecolor='black', alpha=0.7))
    
    def update(frame):
        im.set_array(grids[frame])
        day_text.set_text(f'Día: {frame * 30}')
        return [im, day_text]
    
    anim = animation.FuncAnimation(fig, update, frames=len(grids),
                                  interval=1000/fps, blit=True, repeat=True)
    
    print(f"Guardando animación: {output_file}")
    anim.save(output_file, writer='pillow', fps=fps, dpi=100)
    plt.close()
    print(f"Animación guardada exitosamente")

def create_side_by_side_animation(grids_seq, grids_par, time_seq, time_par, 
                                  output_file, fps=10):
    fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(16, 8))
    
    colors = ['#3b82f6', '#ef4444', '#22c55e', '#374151']
    cmap = ListedColormap(colors)
    
    speedup = time_seq / time_par
    
    ax1.set_title(f'Secuencial\n(Tiempo: {time_seq:.2f}s)', 
                 fontsize=14, fontweight='bold')
    ax2.set_title(f'Paralela\n(Tiempo: {time_par:.2f}s, Speedup: {speedup:.2f}x)', 
                 fontsize=14, fontweight='bold')
    ax1.axis('off')
    ax2.axis('off')
    
    im1 = ax1.imshow(grids_seq[0], cmap=cmap, vmin=0, vmax=3, interpolation='nearest')
    im2 = ax2.imshow(grids_par[0], cmap=cmap, vmin=0, vmax=3, interpolation='nearest')
    
    cbar = fig.colorbar(im2, ax=[ax1, ax2], ticks=[0.375, 1.125, 1.875, 2.625],
                       orientation='horizontal', fraction=0.046, pad=0.04)
    cbar.ax.set_xticklabels(['Susceptible', 'Infectado', 'Recuperado', 'Fallecido'])
    
    day_text = fig.text(0.5, 0.95, '', ha='center', fontsize=16, fontweight='bold',
                       bbox=dict(boxstyle='round', facecolor='white', alpha=0.8))
    
    def update(frame):
        im1.set_array(grids_seq[frame])
        im2.set_array(grids_par[frame])
        day_text.set_text(f'Día: {frame * 30}')
        return [im1, im2]
    
    frames = min(len(grids_seq), len(grids_par))
    anim = animation.FuncAnimation(fig, update, frames=frames,
                                  interval=1000/fps, blit=True, repeat=True)
    
    print(f"Guardando comparación: {output_file}")
    anim.save(output_file, writer='pillow', fps=fps, dpi=100)
    plt.close()
    print(f"Comparación guardada exitosamente")

if __name__ == "__main__":
    print("Generando animaciones...")
    
    seq_dir = "../sequential/output/grid_snapshots"
    par_dir = "../parallel/output/grid_snapshots"
    
    if os.path.exists(seq_dir):
        grids_seq = load_grid_snapshots(seq_dir)
        create_animation(grids_seq, "Simulación SIR - Secuencial",
                        "../results/animation_sequential.gif", fps=10)
    
    if os.path.exists(par_dir):
        grids_par = load_grid_snapshots(par_dir)
        create_animation(grids_par, "Simulación SIR - Paralela",
                        "../results/animation_parallel.gif", fps=10)
    
    if os.path.exists(seq_dir) and os.path.exists(par_dir):
        grids_seq = load_grid_snapshots(seq_dir)
        grids_par = load_grid_snapshots(par_dir)
        
        with open("../sequential/output/execution_time.txt") as f:
            time_seq = float(f.read().strip())
        
        with open("../parallel/output/execution_time.txt") as f:
            lines = f.readlines()
            time_par = float(lines[0].strip())
        
        create_side_by_side_animation(grids_seq, grids_par, time_seq, time_par,
                                     "../results/side_by_side.gif", fps=10)
    
    print("\nTodas las animaciones generadas exitosamente!")