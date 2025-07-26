import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SearchBarComponent } from '../../components/shared/search-bar/search-bar.component';
import { ModalClienteComponent } from '../../components/shared/modal-cliente/modal-cliente.component';
import { NotificationModalComponent } from '../../components/shared/notification-modal/notification-modal.component';
import { ConfirmationModalComponent } from '../../components/shared/confirmation-modal/confirmation-modal.component';
import { ClientesService } from '../../services/clientes.service';
import { NotificationService } from '../../services/notification.service';
import { Cliente } from '../../models/cliente.model';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [CommonModule, FormsModule, SearchBarComponent, ModalClienteComponent, NotificationModalComponent, ConfirmationModalComponent],
  templateUrl: './clientes.component.html',
  styleUrls: ['./clientes.component.scss']
})
export class ClientesComponent implements OnInit {
  clientes: Cliente[] = [];
  filteredClientes: Cliente[] = [];
  isModalOpen: boolean = false;
  modalMode: 'create' | 'edit' = 'create';
  selectedCliente: Cliente | null = null;
  isConfirmationModalOpen: boolean = false;
  clienteToDelete: Cliente | null = null;
  notificationState$;
  
  // Paginacion
  currentPage: number = 1;
  pageSize: number = 15;
  totalPages: number = 0;
  totalItems: number = 0;
  
  // Busqueda
  searchQuery: string = '';
  
  // Usar Math en el template
  Math = Math;

  constructor(
    private clientesService: ClientesService,
    private notificationService: NotificationService
  ) {
    this.notificationState$ = this.notificationService.notification$;
  }

  ngOnInit(): void {
    this.loadClientes();
  }

  loadClientes(page: number = 1): void {
    this.clientesService.getClientes(page, this.pageSize, this.searchQuery).subscribe({
      next: (response) => {
        // Mapear respuesta del API
        this.clientes = response.Data;
        this.filteredClientes = [...this.clientes];
        this.currentPage = response.PageNumber;
        this.totalPages = response.TotalPages;
        this.totalItems = response.TotalRecords;
      },
      error: (error) => {
        console.error('Error al cargar clientes:', error);
      }
    });
  }
  
  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.loadClientes(page);
    }
  }

  onSearch(searchTerm: string): void {
    this.searchQuery = searchTerm;
    this.currentPage = 1;
    this.loadClientes(1);
  }

  onNewCliente(): void {
    this.modalMode = 'create';
    this.selectedCliente = null;
    this.isModalOpen = true;
  }

  onCloseModal(): void {
    this.isModalOpen = false;
    this.selectedCliente = null;
  }

  onSaveCliente(event: {data: any, isPartialUpdate: boolean, modifiedFields: string[]}): void {
    if (this.modalMode === 'create') {
      this.clientesService.createCliente(event.data).subscribe({
        next: (newCliente) => {
          this.clientes.push(newCliente);
          this.filteredClientes = [...this.clientes];
          this.isModalOpen = false;
          this.notificationService.showSuccess('Operación exitosa');
        },
        error: (error) => {
          this.isModalOpen = false;
          const errorMessage = error.error?.detail || error.error?.message || 'Error al crear cliente';
          this.notificationService.showError('Error', errorMessage);
        }
      });
    } else if (this.modalMode === 'edit' && this.selectedCliente && this.selectedCliente.ClienteId) {
      // Usar PATCH o PUT según el evento
      const updateMethod = event.isPartialUpdate 
        ? this.clientesService.patchCliente(this.selectedCliente.ClienteId, event.data)
        : this.clientesService.updateCliente(this.selectedCliente.ClienteId, event.data);
      
      updateMethod.subscribe({
        next: (updatedCliente) => {
          const index = this.clientes.findIndex(c => c.ClienteId === this.selectedCliente!.ClienteId);
          if (index !== -1) {
            this.clientes[index] = updatedCliente;
            this.filteredClientes = [...this.clientes];
          }
          this.isModalOpen = false;
          this.notificationService.showSuccess('Operación exitosa');
        },
        error: (error) => {
          this.isModalOpen = false;
          this.selectedCliente = null;
          const errorMessage = error.error?.detail || error.error?.message || 'Error al actualizar cliente';
          this.notificationService.showError('Error', errorMessage);
        }
      });
    }
  }

  onEditCliente(cliente: Cliente): void {
    this.modalMode = 'edit';
    this.selectedCliente = cliente;
    this.isModalOpen = true;
  }

  closeNotification(): void {
    this.notificationService.close();
  }

  onDeleteCliente(cliente: Cliente): void {
    if (!cliente.Estado) {
      this.notificationService.showError('Error', 'No se puede eliminar un cliente inactivo');
      return;
    }
    this.clienteToDelete = cliente;
    this.isConfirmationModalOpen = true;
  }

  onConfirmDelete(): void {
    if (this.clienteToDelete && this.clienteToDelete.ClienteId) {
      this.clientesService.deleteCliente(this.clienteToDelete.ClienteId).subscribe({
        next: () => {
          this.isConfirmationModalOpen = false;
          this.clienteToDelete = null;
          this.notificationService.showSuccess('Cliente eliminado exitosamente');
          // Cargar la lista de clientes nuevamente
          this.loadClientes(this.currentPage);
        },
        error: (error) => {
          this.isConfirmationModalOpen = false;
          this.clienteToDelete = null;
          const errorMessage = error.error?.detail || error.error?.message || 'Error al eliminar cliente';
          this.notificationService.showError('Error', errorMessage);
        }
      });
    }
  }

  onCancelDelete(): void {
    this.isConfirmationModalOpen = false;
    this.clienteToDelete = null;
  }
  
  getPageNumbers(): (number | string)[] {
    const pages: (number | string)[] = [];
    const maxPagesToShow = 5;
    
    if (this.totalPages <= maxPagesToShow) {
      // Mostrar todo si se puede
      for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i);
      }
    } else {
      // Siempre mostrar la primera pagina
      pages.push(1);
      
      // Calcular rango de paginas
      let start = Math.max(2, this.currentPage - 1);
      let end = Math.min(this.totalPages - 1, this.currentPage + 1);
      
      if (start > 2) {
        pages.push('...');
      }
      
      for (let i = start; i <= end; i++) {
        pages.push(i);
      }
      
      if (end < this.totalPages - 1) {
        pages.push('...');
      }
      
      // Mostrar ultima pagina
      pages.push(this.totalPages);
    }
    
    return pages;
  }
  
  onPageSizeChange(): void {
    // Volver a la primera pagina si cambia el tamaño
    this.currentPage = 1;
    this.loadClientes(1);
  }
}