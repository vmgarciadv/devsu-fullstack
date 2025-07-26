import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NotificationModalComponent } from '../../components/shared/notification-modal/notification-modal.component';
import { ReportesService } from '../../services/reportes.service';
import { ClientesService } from '../../services/clientes.service';
import { NotificationService } from '../../services/notification.service';
import { Reporte } from '../../models/reporte.model';
import { Cliente } from '../../models/cliente.model';

@Component({
  selector: 'app-reportes',
  standalone: true,
  imports: [CommonModule, FormsModule, NotificationModalComponent],
  templateUrl: './reportes.component.html',
  styleUrl: './reportes.component.scss'
})
export class ReportesComponent implements OnInit {
  reportes: Reporte[] = [];
  clientes: Cliente[] = [];
  notificationState$;
  
  // Filter controls
  selectedCliente: string = '';
  useDateRange: boolean = false;
  singleDate: string = '';
  fechaInicio: string = '';
  fechaFin: string = '';
  
  constructor(
    private reportesService: ReportesService,
    private clientesService: ClientesService,
    private notificationService: NotificationService
  ) {
    this.notificationState$ = this.notificationService.notification$;
  }

  ngOnInit(): void {
    this.loadClientes();
  }

  loadClientes(): void {
    this.clientesService.getAllClientes().subscribe({
      next: (clientes) => {
        this.clientes = clientes;
      },
      error: (error) => {
        console.error('Error al cargar clientes:', error);
      }
    });
  }

  onGenerateReport(): void {
    if (!this.selectedCliente) {
      this.notificationService.showError('Error', 'Debe seleccionar un cliente');
      return;
    }

    if (!this.useDateRange && !this.singleDate) {
      this.notificationService.showError('Error', 'Debe seleccionar una fecha');
      return;
    }

    if (this.useDateRange && (!this.fechaInicio || !this.fechaFin)) {
      this.notificationService.showError('Error', 'Debe seleccionar ambas fechas del rango');
      return;
    }

    const fecha = this.useDateRange ? undefined : this.singleDate;
    const fechaInicio = this.useDateRange ? this.fechaInicio : undefined;
    const fechaFin = this.useDateRange ? this.fechaFin : undefined;

    this.reportesService.getReportes(this.selectedCliente, fecha, fechaInicio, fechaFin).subscribe({
      next: (data) => {
        this.reportes = data;
        if (data.length === 0) {
          this.notificationService.showError('Sin datos', 'No se encontraron movimientos para los criterios seleccionados');
        }
      },
      error: (error) => {
        const errorMessage = error.error?.detail || error.error?.message || 'Error al generar reporte';
        this.notificationService.showError('Error', errorMessage);
      }
    });
  }

  toggleDateMode(): void {
    this.singleDate = '';
    this.fechaInicio = '';
    this.fechaFin = '';
  }

  closeNotification(): void {
    this.notificationService.close();
  }

  downloadJSON(): void {
    const dataStr = JSON.stringify(this.reportes, null, 2);
    const blob = new Blob([dataStr], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `reporte_${this.selectedCliente}_${new Date().toISOString().split('T')[0]}.json`;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  downloadPDF(): void {
    // Obtener Periodo del PDF segun las fechas seleccionadas
    let periodText = '';
    if (this.useDateRange && this.fechaInicio && this.fechaFin) {
      // Formatear fechas para evitar problemas de zona horaria
      // Fecha inicio - Fecha fin
      const [yearStart, monthStart, dayStart] = this.fechaInicio.split('-');
      const [yearEnd, monthEnd, dayEnd] = this.fechaFin.split('-');
      periodText = `${dayStart}/${monthStart}/${yearStart} - ${dayEnd}/${monthEnd}/${yearEnd}`;
    } else if (!this.useDateRange && this.singleDate) {
      // Fecha única
      const [year, month, day] = this.singleDate.split('-');
      periodText = `${day}/${month}/${year}`;
    }

    // Tabla con HTML y estilos para el PDF
    let htmlContent = `
      <html>
        <head>
          <title>Estado de Cuenta</title>
          <style>
            body { font-family: Arial, sans-serif; margin: 20px; }
            h1 { text-align: center; color: #333; }
            .info { margin-bottom: 20px; }
            .info p { margin: 5px 0; }
            table { width: 100%; border-collapse: collapse; margin-top: 20px; }
            th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
            th { background-color: #f5f5f5; font-weight: bold; }
            tr:hover { background-color: #f9f9f9; }
            .debit { color: #c62828; }
            .credit { color: #2e7d32; }
            .status-active { background-color: #e8f5e9; color: #2e7d32; padding: 4px 8px; border-radius: 4px; }
            .status-inactive { background-color: #ffebee; color: #c62828; padding: 4px 8px; border-radius: 4px; }
          </style>
        </head>
        <body>
          <h1>Estado de Cuenta</h1>
          <div class="info">
            <p><strong>Cliente:</strong> ${this.selectedCliente}</p>
            <p><strong>Fecha de generación:</strong> ${new Date().toLocaleDateString('es-ES')}</p>
            <p><strong>Período:</strong> ${periodText}</p>
          </div>
          <table>
            <thead>
              <tr>
                <th>Fecha</th>
                <th>Número Cuenta</th>
                <th>Tipo Cuenta</th>
                <th>Saldo Inicial</th>
                <th>Estado</th>
                <th>Movimiento</th>
                <th>Saldo Disponible</th>
              </tr>
            </thead>
            <tbody>
    `;

    this.reportes.forEach(reporte => {
      // Formatear fecha
      let fecha = reporte.Fecha;
      if (fecha && fecha.includes('-')) {
        const datePart = fecha.split('T')[0]; // Eliminar hora
        const [year, month, day] = datePart.split('-');
        fecha = `${day}/${month}/${year}`;
      }
      
      const movimientoClass = reporte.TotalMovimientos < 0 ? 'debit' : reporte.TotalMovimientos > 0 ? 'credit' : '';
      const estadoClass = reporte.Estado ? 'status-active' : 'status-inactive';
      const estadoText = reporte.Estado ? 'Activa' : 'Inactiva';
      
      htmlContent += `
        <tr>
          <td>${fecha}</td>
          <td>${reporte.NumeroCuenta}</td>
          <td>${reporte.TipoCuenta}</td>
          <td>${reporte.SaldoInicial < 0 ? '-' : ''}$${Math.abs(reporte.SaldoInicial).toFixed(2)}</td>
          <td><span class="${estadoClass}">${estadoText}</span></td>
          <td class="${movimientoClass}">${reporte.TotalMovimientos < 0 ? '-' : ''}$${Math.abs(reporte.TotalMovimientos).toFixed(2)}</td>
          <td>${reporte.SaldoDisponible < 0 ? '-' : ''}$${Math.abs(reporte.SaldoDisponible).toFixed(2)}</td>
        </tr>
      `;
    });

    htmlContent += `
            </tbody>
          </table>
        </body>
      </html>
    `;

    // Abrir en nueva ventana
    const printWindow = window.open('', '_blank');
    if (printWindow) {
      printWindow.document.write(htmlContent);
      printWindow.document.close();
      printWindow.print();
    }
  }
}
