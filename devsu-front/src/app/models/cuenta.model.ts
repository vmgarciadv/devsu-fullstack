export interface Cuenta {
  CuentaId?: number;
  NumeroCuenta: string;
  TipoCuenta: string;
  SaldoInicial: number;
  Estado: boolean;
  ClienteId: number;
  NombreCliente?: string;
}