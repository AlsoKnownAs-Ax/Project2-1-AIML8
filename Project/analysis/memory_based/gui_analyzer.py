import tkinter as tk
from tkinter import ttk
import pandas as pd
from pathlib import Path

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

def main():
    root = tk.Tk()
    app = CSVAnalyzerGUI(root)
    root.mainloop()

if __name__ == "__main__":
    main()
