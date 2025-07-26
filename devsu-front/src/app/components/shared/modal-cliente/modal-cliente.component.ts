import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Cliente } from '../../../models/cliente.model';

@Component({
  selector: 'app-modal-cliente',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './modal-cliente.component.html',
  styleUrls: ['./modal-cliente.component.scss']
})
export class ModalClienteComponent implements OnChanges {
  @Input() isOpen: boolean = false;
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() clienteToEdit: Cliente | null = null;
  @Output() closeModal = new EventEmitter<void>();
  @Output() saveCliente = new EventEmitter<{data: any, isPartialUpdate: boolean, modifiedFields: string[]}>();

  cliente: Cliente & { Contrasena: string } = {
    Nombre: '',
    Genero: '',
    Edad: 0,
    Identificacion: '',
    Direccion: '',
    Telefono: '',
    Estado: true,
    Contrasena: ''
  };

  originalCliente: Cliente & { Contrasena: string } | null = null;
  modifiedFields: Set<string> = new Set();

  touched = {
    Nombre: false,
    Identificacion: false,
    Genero: false,
    Edad: false,
    Direccion: false,
    Telefono: false,
    Contrasena: false
  };

  errors: {
    Nombre?: string;
    Identificacion?: string;
    Genero?: string;
    Edad?: string;
    Direccion?: string;
    Telefono?: string;
    Contrasena?: string;
  } = {};

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen'] && this.isOpen) {
      this.touched = {
        Nombre: false,
        Identificacion: false,
        Genero: false,
        Edad: false,
        Direccion: false,
        Telefono: false,
        Contrasena: false
      };
      this.errors = {};
      this.modifiedFields.clear();
      
      if (this.mode === 'edit' && this.clienteToEdit) {
        this.cliente = {
          ...this.clienteToEdit,
 
          Genero: this.clienteToEdit.Genero === 'M' ? 'Masculino' : 
                  this.clienteToEdit.Genero === 'F' ? 'Femenino' : 
                  this.clienteToEdit.Genero,
          Contrasena: ''
        };
        this.originalCliente = { ...this.cliente };
      } else if (this.mode === 'create') {
        this.resetForm();
      }
    }
  }

  onClose(): void {
    this.closeModal.emit();
    this.resetForm();
  }

  onSave(): void {
    (Object.keys(this.touched) as Array<keyof typeof this.touched>).forEach(field => {
      this.touched[field] = true;
      this.validateField(field);
    });
    
    if (this.isFormValid()) {
      const { Estado, ...clienteData } = this.cliente;
      
      if (this.mode === 'create') {
        const dataToSend = {
          ...clienteData,
          Genero: clienteData.Genero === 'Masculino' ? 'M' : clienteData.Genero === 'Femenino' ? 'F' : clienteData.Genero
        };
        this.saveCliente.emit({
          data: dataToSend,
          isPartialUpdate: false,
          modifiedFields: []
        });
      } else {
        const isPartialUpdate = this.modifiedFields.size > 0 && this.modifiedFields.size < 6;
        
        let dataToSend: any = {};
        
        if (isPartialUpdate) {
          this.modifiedFields.forEach(field => {
            if (field === 'Genero') {
              dataToSend[field] = this.cliente[field] === 'Masculino' ? 'M' : this.cliente[field] === 'Femenino' ? 'F' : this.cliente[field];
            } else if (field !== 'Contrasena' || this.cliente.Contrasena.trim() !== '') {
              dataToSend[field] = this.cliente[field as keyof typeof this.cliente];
            }
          });
        } else {
          dataToSend = {
            ...clienteData,
            Genero: clienteData.Genero === 'Masculino' ? 'M' : clienteData.Genero === 'Femenino' ? 'F' : clienteData.Genero
          };
          if (!dataToSend.Contrasena) {
            delete dataToSend.Contrasena;
          }
        }
        
        this.saveCliente.emit({
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
    
    if (this.mode === 'edit' && this.originalCliente) {
      const fieldValue = this.cliente[field as keyof typeof this.cliente];
      const originalValue = this.originalCliente[field as keyof typeof this.originalCliente];
      
      if (field === 'Edad') {
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
      case 'Nombre':
        if (!this.cliente.Nombre.trim()) {
          this.errors.Nombre = 'El nombre es requerido';
        } else if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/.test(this.cliente.Nombre)) {
          this.errors.Nombre = 'Solo se permiten letras';
        } else if (this.cliente.Nombre.length > 30) {
          this.errors.Nombre = 'Máximo 30 caracteres';
        }
        break;

      case 'Identificacion':
        if (!this.cliente.Identificacion.trim()) {
          this.errors.Identificacion = 'La identificación es requerida';
        } else if (!/^\d+$/.test(this.cliente.Identificacion)) {
          this.errors.Identificacion = 'Solo se permiten números';
        } else if (this.cliente.Identificacion.length > 10) {
          this.errors.Identificacion = 'Máximo 10 dígitos';
        }
        break;

      case 'Edad':
        if (!this.cliente.Edad || this.cliente.Edad <= 0) {
          this.errors.Edad = 'La edad es requerida';
        } else if (!/^\d+$/.test(this.cliente.Edad.toString())) {
          this.errors.Edad = 'Solo se permiten números';
        }
        break;

      case 'Direccion':
        if (!this.cliente.Direccion.trim()) {
          this.errors.Direccion = 'La dirección es requerida';
        } else if (this.cliente.Direccion.length > 100) {
          this.errors.Direccion = 'Máximo 100 caracteres';
        }
        break;

      case 'Telefono':
        if (!this.cliente.Telefono.trim()) {
          this.errors.Telefono = 'El teléfono es requerido';
        } else if (!/^\d+$/.test(this.cliente.Telefono)) {
          this.errors.Telefono = 'Solo se permiten números';
        } else if (this.cliente.Telefono.length > 11) {
          this.errors.Telefono = 'Máximo 11 dígitos';
        }
        break;

      case 'Contrasena':
        if (this.mode === 'create' || this.cliente.Contrasena.trim() !== '') {
          if (this.mode === 'create' && !this.cliente.Contrasena.trim()) {
            this.errors.Contrasena = 'La contraseña es requerida';
          } else if (this.cliente.Contrasena.trim() && !/[A-Z]/.test(this.cliente.Contrasena)) {
            this.errors.Contrasena = 'Debe contener al menos una mayúscula';
          } else if (this.cliente.Contrasena.trim() && !/[0-9]/.test(this.cliente.Contrasena)) {
            this.errors.Contrasena = 'Debe contener al menos un número';
          } else if (this.cliente.Contrasena.trim() && !/[!@#$%^&*(),.?":{}|<>]/.test(this.cliente.Contrasena)) {
            this.errors.Contrasena = 'Debe contener al menos un carácter especial';
          }
        }
        break;

      case 'Genero':
        if (!this.cliente.Genero) {
          this.errors.Genero = 'El género es requerido';
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
    
    // Checkear que todos los campos requeridos estén completos
    const baseValidation = !hasErrors &&
           this.cliente.Nombre.trim() !== '' &&
           this.cliente.Identificacion.trim() !== '' &&
           this.cliente.Genero.trim() !== '' &&
           this.cliente.Edad > 0 &&
           this.cliente.Direccion.trim() !== '' &&
           this.cliente.Telefono.trim() !== '';
    
    // La contraseña solo es requerida para la creación
    if (this.mode === 'create') {
      return baseValidation && this.cliente.Contrasena.trim() !== '';
    }
    
    // Checkear si hay cambios al editar
    return baseValidation && this.hasChanges();
  }

  private resetForm(): void {
    this.cliente = {
      Nombre: '',
      Genero: '',
      Edad: 0,
      Identificacion: '',
      Direccion: '',
      Telefono: '',
      Estado: true,
      Contrasena: ''
    };
    
    // Limpiar
    (Object.keys(this.touched) as Array<keyof typeof this.touched>).forEach(key => {
      this.touched[key] = false;
    });

    this.errors = {};
    this.originalCliente = null;
    this.modifiedFields.clear();
  }
}