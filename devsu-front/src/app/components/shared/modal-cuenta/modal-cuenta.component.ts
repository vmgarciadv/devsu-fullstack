import { Component, EventEmitter, Input, Output, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Cuenta } from '../../../models/cuenta.model';
import { Cliente } from '../../../models/cliente.model';
import { ClientesService } from '../../../services/clientes.service';

@Component({
  selector: 'app-modal-cuenta',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './modal-cuenta.component.html',
  styleUrls: ['./modal-cuenta.component.scss']
})
export class ModalCuentaComponent implements OnInit, OnChanges {
  @Input() isOpen: boolean = false;
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() cuentaToEdit: Cuenta | null = null;
  @Output() closeModal = new EventEmitter<void>();
  @Output() saveCuenta = new EventEmitter<{data: any, isPartialUpdate: boolean, modifiedFields: string[]}>();

  cuenta: Omit<Cuenta, 'SaldoInicial'> & { nombreCliente: string; SaldoInicial: string | number } = {
    NumeroCuenta: '',
    TipoCuenta: '',
    SaldoInicial: '', // Empty string to handle text input
    Estado: true,
    ClienteId: 0,
    nombreCliente: ''
  };

  clientes: Cliente[] = [];
  activeClientes: Cliente[] = [];
  originalCuenta: Omit<Cuenta, 'SaldoInicial'> & { nombreCliente: string; SaldoInicial: string | number } | null = null;
  modifiedFields: Set<string> = new Set();

  touched = {
    NumeroCuenta: false,
    TipoCuenta: false,
    SaldoInicial: false,
    nombreCliente: false
  };

  errors: {
    NumeroCuenta?: string;
    TipoCuenta?: string;
    SaldoInicial?: string;
    nombreCliente?: string;
  } = {};

  constructor(private clientesService: ClientesService) {}

  ngOnInit(): void {
    this.loadClientes();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen'] && this.isOpen) {
      this.touched = {
        NumeroCuenta: false,
        TipoCuenta: false,
        SaldoInicial: false,
        nombreCliente: false
      };
      this.errors = {};
      this.modifiedFields.clear();
      
      if (this.mode === 'edit' && this.cuentaToEdit) {
        this.cuenta = {
          ...this.cuentaToEdit,
          nombreCliente: this.cuentaToEdit.NombreCliente || '',
          SaldoInicial: this.cuentaToEdit.SaldoInicial.toString()
        };
        this.originalCuenta = { ...this.cuenta };

        this.loadClientes();
      } else if (this.mode === 'create') {
        this.resetForm();
      }
    }
  }

  loadClientes(): void {
    this.clientesService.getAllClientes().subscribe({
      next: (clientes) => {
        this.clientes = clientes;
        // Filtrar clientes activos
        this.activeClientes = clientes.filter(cliente => cliente.Estado === true);
        
        // Agregar clientes inactivos para la edicion
        if (this.mode === 'edit' && this.cuentaToEdit && this.cuenta.nombreCliente) {
          const currentClientExists = this.activeClientes.some(
            cliente => cliente.Nombre === this.cuenta.nombreCliente
          );
          
          if (!currentClientExists) {
            const inactiveClient = clientes.find(
              cliente => cliente.Nombre === this.cuenta.nombreCliente && !cliente.Estado
            );
            
            if (inactiveClient) {
              this.activeClientes = [...this.activeClientes, inactiveClient];
            }
          }
        }
      },
      error: (error) => {
        console.error('Error al cargar clientes:', error);
      }
    });
  }

  onClose(): void {
    this.closeModal.emit();
    this.resetForm();
  }

  onSave(): void {
    // Validar todos los campos
    (Object.keys(this.touched) as Array<keyof typeof this.touched>).forEach(field => {
      this.touched[field] = true;
      this.validateField(field);
    });
    
    if (this.isFormValid()) {
      if (this.mode === 'create') {
        // Enviar solo los campos necesarios
        const dataToSend = {
          tipoCuenta: this.cuenta.TipoCuenta,
          saldoInicial: Number(this.cuenta.SaldoInicial),
          nombreCliente: this.cuenta.nombreCliente
        };
        this.saveCuenta.emit({
          data: dataToSend,
          isPartialUpdate: false,
          modifiedFields: []
        });
      } else {
        // Comprobar campos modificados para usar PUT o PATCH
        const isPartialUpdate = this.modifiedFields.size > 0 && this.modifiedFields.size < 4;
        
        let dataToSend: any = {};
        
        if (isPartialUpdate) {
          this.modifiedFields.forEach(field => {
            if (field === 'TipoCuenta') {
              dataToSend.tipoCuenta = this.cuenta.TipoCuenta;
            } else if (field === 'SaldoInicial') {
              dataToSend.saldoInicial = Number(this.cuenta.SaldoInicial);
            } else if (field === 'nombreCliente') {
              dataToSend.nombreCliente = this.cuenta.nombreCliente;
            }
          });
        } else {
          dataToSend = {
            tipoCuenta: this.cuenta.TipoCuenta,
            saldoInicial: Number(this.cuenta.SaldoInicial),
            nombreCliente: this.cuenta.nombreCliente
          };
        }
        
        this.saveCuenta.emit({
          data: dataToSend,
          isPartialUpdate,
          modifiedFields: Array.from(this.modifiedFields)
        });
      }
    }
  }

  onFieldChange(field: keyof typeof this.touched): void {
    this.touched[field] = true;
    this.validateField(field);
    
    if (this.mode === 'edit' && this.originalCuenta) {
      const fieldValue = this.cuenta[field as keyof typeof this.cuenta];
      const originalValue = this.originalCuenta[field as keyof typeof this.originalCuenta];
      
      if (field === 'SaldoInicial') {
        if (Number(fieldValue) !== Number(originalValue)) {
          this.modifiedFields.add(field);
        } else {
          this.modifiedFields.delete(field);
        }
      } else if (fieldValue !== originalValue) {
        this.modifiedFields.add(field);
      } else {
        this.modifiedFields.delete(field);
      }
    }
  }


  validateField(field: keyof typeof this.touched): void {
    this.errors[field] = '';

    switch (field) {
      case 'NumeroCuenta':
        // No se puede editar, no hace falta validacion
        break;

      case 'TipoCuenta':
        if (!this.cuenta.TipoCuenta) {
          this.errors.TipoCuenta = 'El tipo de cuenta es requerido';
        }
        break;

      case 'SaldoInicial':
        const saldoValue = String(this.cuenta.SaldoInicial).trim();

        if (saldoValue && (!/^\d*\.?\d*$/.test(saldoValue) || saldoValue === '.')) {
          this.errors.SaldoInicial = 'Solo se permiten números';
        } else if (!saldoValue) {
          this.errors.SaldoInicial = 'El saldo inicial es requerido';
        } else if (Number(saldoValue) <= 0) {
          this.errors.SaldoInicial = 'El saldo inicial debe ser mayor a 0';
        }
        break;

      case 'nombreCliente':
        if (!this.cuenta.nombreCliente.trim()) {
          this.errors.nombreCliente = 'El cliente es requerido';
        }
        break;
    }
  }

  hasChanges(): boolean {
    return this.modifiedFields.size > 0;
  }

  isFormValid(): boolean {
    const hasErrors = Object.entries(this.errors).some(([field, error]) => {
      return this.touched[field as keyof typeof this.touched] && error;
    });
    
    const saldoStr = String(this.cuenta.SaldoInicial).trim();
    const baseValidation = !hasErrors &&
           this.cuenta.TipoCuenta !== '' &&
           saldoStr !== '' &&
           /^\d*\.?\d*$/.test(saldoStr) &&
           Number(saldoStr) > 0 &&
           this.cuenta.nombreCliente.trim() !== '';
    
    if (this.mode === 'create') {
      return baseValidation;
    }
    
    return baseValidation && this.hasChanges();
  }

  private resetForm(): void {
    this.cuenta = {
      NumeroCuenta: '',
      TipoCuenta: '',
      SaldoInicial: '',
      Estado: true,
      ClienteId: 0,
      nombreCliente: ''
    };
    
    (Object.keys(this.touched) as Array<keyof typeof this.touched>).forEach(key => {
      this.touched[key] = false;
    });
    
    this.errors = {};
    this.originalCuenta = null;
    this.modifiedFields.clear();
  }
}