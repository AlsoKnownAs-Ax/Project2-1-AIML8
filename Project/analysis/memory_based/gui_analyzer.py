import tkinter as tk
from tkinter import ttk
import pandas as pd
from pathlib import Path
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg

class GraphWindow:
    def __init__(self, csv_type):
        self.window = tk.Toplevel()
        self.window.title(f"{csv_type} Comparison")
        self.window.geometry("800x600")
        
        # Create main container
        self.container = ttk.Frame(self.window)
        self.container.pack(fill=tk.BOTH, expand=True)
        
        # Create figure and plot
        self.fig, self.ax = plt.subplots(figsize=(10, 6))
        self.canvas = FigureCanvasTkAgg(self.fig, master=self.container)
        self.canvas.get_tk_widget().pack(fill=tk.BOTH, expand=True)
        
        # Add Save button
        self.save_button = ttk.Button(self.container, text="Save Graph", 
                                    command=lambda: self.save_plot(csv_type))
        self.save_button.pack(pady=5)
        
        # Plot data
        self.plot_comparison(csv_type)

    def plot_comparison(self, csv_type):
        try:
            # Get the directory path where the script is located
            base_path = Path(__file__).parent

            # Load all three datasets using proper paths
            df_default = pd.read_csv(base_path / f"{csv_type}.csv")
            df_50m = pd.read_csv(base_path / f"{csv_type}_50.csv")
            df_20m = pd.read_csv(base_path / f"{csv_type}_20.csv")

            # Convert steps to millions for better readability
            df_default['Step'] = df_default['Step'] / 1000000
            df_50m['Step'] = df_50m['Step'] / 1000000
            df_20m['Step'] = df_20m['Step'] / 1000000

            # Apply rolling mean to smooth the curves
            window_size = 50  # Adjust this value to control smoothness
            df_default['Value'] = df_default['Value'].rolling(window=window_size, center=True).mean()
            df_50m['Value'] = df_50m['Value'].rolling(window=window_size, center=True).mean()
            df_20m['Value'] = df_20m['Value'].rolling(window=window_size, center=True).mean()

            # Plot the smoothed data
            self.ax.plot(df_default['Step'], df_default['Value'], label='Default', linewidth=2)
            self.ax.plot(df_50m['Step'], df_50m['Value'], label='50 Million', linewidth=2)
            self.ax.plot(df_20m['Step'], df_20m['Value'], label='20 Million', linewidth=2)

            self.ax.set_title(f'{csv_type} Comparison')
            self.ax.set_xlabel('Steps (Million)')
            self.ax.set_ylabel('Value')
            self.ax.legend()
            self.ax.grid(True, alpha=0.3)
            
            # Make the plot look nicer
            plt.style.use('seaborn')
            self.fig.tight_layout()
            
            self.canvas.draw()
        except Exception as e:
            tk.messagebox.showerror("Error", f"Error plotting data: {str(e)}")

    def save_plot(self, csv_type):
        try:
            # Define the filename based on CSV type
            filename_map = {
                "ELO": "ELO_comparison.png",
                "Entropy": "Entropy_comparison.png",
                "Group-Cumulative-Reward": "Group_Cumulative_reward_comparison.png",
                "Policy-Loss": "Policy_Loss_comparison.png",
                "Value-Loss": "Value_Loss_comparison.png"
            }
            
            filename = filename_map.get(csv_type)
            if filename:
                # Get the directory path where the script is located
                save_path = Path(__file__).parent / filename
                self.fig.savefig(save_path)
                tk.messagebox.showinfo("Success", f"Graph saved as {filename}")
            else:
                tk.messagebox.showerror("Error", "Invalid CSV type for saving")
                
        except Exception as e:
            tk.messagebox.showerror("Error", f"Error saving plot: {str(e)}")

class CSVAnalyzerGUI:
    def __init__(self, root):
        self.root = root
        self.root.title("CSV File Analyzer")
        self.root.geometry("1200x600")

        # Create main frame
        self.main_frame = ttk.Frame(root, padding="10")
        self.main_frame.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))

        # Create frames for different sections
        self.button_frame = ttk.Frame(self.main_frame)
        self.button_frame.grid(row=0, column=0, columnspan=2, pady=10)

        self.data_frame = ttk.Frame(self.main_frame)
        self.data_frame.grid(row=1, column=0, padx=5)

        self.stats_frame = ttk.Frame(self.main_frame)
        self.stats_frame.grid(row=1, column=1, padx=5)

        # Create buttons
        ttk.Button(self.button_frame, text="DEFAULT", command=lambda: self.load_data("default")).grid(row=0, column=0, padx=5)
        ttk.Button(self.button_frame, text="50 MILLION", command=lambda: self.load_data("50m")).grid(row=0, column=1, padx=5)
        ttk.Button(self.button_frame, text="20 MILLION", command=lambda: self.load_data("20m")).grid(row=0, column=2, padx=5)
        ttk.Button(self.button_frame, text="Return", command=self.clear_display).grid(row=0, column=3, padx=5)
        ttk.Button(self.button_frame, text="Exit", command=root.quit).grid(row=0, column=4, padx=5)

        # Create dropdown for selecting CSV type
        self.csv_type = tk.StringVar()
        self.csv_types = ["Entropy", "ELO", "Group-Cumulative-Reward", "Policy-Loss", "Value-Loss"]
        self.csv_dropdown = ttk.Combobox(self.button_frame, textvariable=self.csv_type, values=self.csv_types)
        self.csv_dropdown.grid(row=0, column=5, padx=5)
        self.csv_dropdown.set("Select CSV Type")

        # Add Compare button
        ttk.Button(self.button_frame, text="Compare Graphs", 
                  command=self.show_comparison).grid(row=0, column=6, padx=5)

        # Create text widgets for displaying data and stats
        self.data_text = tk.Text(self.data_frame, width=70, height=30)
        self.data_text.pack()

        self.stats_text = tk.Text(self.stats_frame, width=40, height=30)
        self.stats_text.pack()

    def clear_display(self):
        self.data_text.delete(1.0, tk.END)
        self.stats_text.delete(1.0, tk.END)
        self.csv_dropdown.set("Select CSV Type")

    def load_data(self, version):
        if self.csv_type.get() == "Select CSV Type":
            self.data_text.delete(1.0, tk.END)
            self.data_text.insert(tk.END, "Please select a CSV type first")
            return

        # Clear previous content
        self.data_text.delete(1.0, tk.END)
        self.stats_text.delete(1.0, tk.END)

        # Construct file path
        file_suffix = ""
        if version == "50m":
            file_suffix = "_50"
        elif version == "20m":
            file_suffix = "_20"

        filename = f"{self.csv_type.get()}{file_suffix}.csv"
        file_path = Path(__file__).parent / filename

        try:
            # Read CSV file
            df = pd.read_csv(file_path)

            # Display all rows
            self.data_text.insert(tk.END, df.to_string())

            # Calculate and display statistics
            stats = df.describe()
            stats_text = f"\nStatistics for {filename}:\n\n"
            stats_text += f"Mean: {stats['Value']['mean']:.6f}\n"
            stats_text += f"Std Dev: {stats['Value']['std']:.6f}\n"
            stats_text += f"Min: {stats['Value']['min']:.6f}\n"
            stats_text += f"Max: {stats['Value']['max']:.6f}\n"
            
            self.stats_text.insert(tk.END, stats_text)

        except FileNotFoundError:
            self.data_text.insert(tk.END, f"Error: File {filename} not found")
        except Exception as e:
            self.data_text.insert(tk.END, f"Error loading file: {str(e)}")

    def show_comparison(self):
        if self.csv_type.get() == "Select CSV Type":
            tk.messagebox.showwarning("Warning", "Please select a CSV type first")
            return
        GraphWindow(self.csv_type.get())

def main():
    root = tk.Tk()
    app = CSVAnalyzerGUI(root)
    root.mainloop()

if __name__ == "__main__":
    main()
