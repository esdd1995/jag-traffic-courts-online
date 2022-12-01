import { Injectable } from '@angular/core';
import { UserRepresentation as UserRepresentationBase } from 'app/api';
import { AppRoutes } from 'app/app.routes';
import { KeycloakService } from 'keycloak-angular';
import { KeycloakService as KeycloakAPIService } from 'app/api'
import { KeycloakProfile as KeycloakProfileJS } from 'keycloak-js';
import { BehaviorSubject, from, Observable, map, catchError } from 'rxjs';
import { LoggerService } from '@core/services/logger.service';
import { ToastService } from '@core/services/toast.service';
import { ConfigService } from '@config/config.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private _isLoggedIn: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(null);
  private _userProfile: BehaviorSubject<KeycloakProfile> = new BehaviorSubject<KeycloakProfile>(null);

  private site: string = "staff-api";
  private roles = [
    { name: "judicial-justice", redirectUrl: AppRoutes.JJWORKBENCH },
    { name: "vtc-staff", redirectUrl: AppRoutes.TICKET },
  ]

  constructor(
    private keycloak: KeycloakService,
    private keycloakAPI: KeycloakAPIService,
    private toastService: ToastService,
    private logger: LoggerService,
    private configService: ConfigService,
  ) { }

  public checkAuth(): Observable<boolean> {
    return from(this.keycloak.isLoggedIn())
      .pipe(
        map((response: boolean) => {
          if (response) {
            this.loadUserProfile().subscribe(() => {
              this._isLoggedIn.next(response);
              return response;
            });
          } else {
            this._isLoggedIn.next(response);
            return response;
          }
        })
      );
  }

  public get isLoggedIn$(): Observable<boolean> {
    return this._isLoggedIn.asObservable();
  }

  public get isLoggedIn(): boolean {
    return this._isLoggedIn.value;
  }

  public loadUserProfile(): Observable<KeycloakProfile> {
    return from(this.keycloak.loadUserProfile())
      .pipe(
        map((response: KeycloakProfile) => {
          response.idir = this.splitIDIR(this.keycloak.getUsername());
          response.fullName = this.getFullName(this.userProfile);
          this._userProfile.next(response);
          return response;
        })
      )
  }

  public get userProfile$(): Observable<KeycloakProfile> {
    return this._userProfile.asObservable();
  }

  public get userProfile(): KeycloakProfile {
    return this._userProfile.value;
  }

  private splitIDIR(fullIDIR): string {
    return fullIDIR?.split("@")[0];
  }

  private getFullName(user: UserRepresentation | KeycloakProfile): string {
    return (user?.firstName ? user?.firstName + " " : "") + (user?.lastName ? user?.lastName : "");
  }

  public login() {
    this.keycloak.login({ redirectUri: window.location.toString() });
  }

  public logout() {
    this.keycloak.logout();
    this._isLoggedIn.next(false);
    this._userProfile.next(null);
  }

  public getRedirectUrl(): string {
    var result;
    this.roles.forEach(r => {
      if (this.keycloak.isUserInRole(r.name, this.site)) {
        result = r.redirectUrl;
      }
    })
    if (!result) {
      result = AppRoutes.UNAUTHORIZED;
    }
    return result;
  }

  public checkRole(role: string): boolean {
    return this.keycloak.isUserInRole(role, this.site);
  }

  public getUsersInGroup(group: string): Observable<Array<UserRepresentation>> {
    return this.keycloakAPI.apiKeycloakGroupNameUsersGet(group)
      .pipe(
        map((response: UserRepresentation[]) => {
          this.logger.info('KeycloakService::getUsersInGroup', response)
          response.forEach((user: UserRepresentation) => {
            user.idir = this.splitIDIR(user.username);
            user.fullName = this.getFullName(user);
          })
          return response ? response : null
        }),
        catchError((error: any) => {
          var errorMsg = error.error.detail != null ? error.error.detail : this.configService.keycloak_error;
          this.toastService.openErrorToast(errorMsg);
          this.toastService.openErrorToast(this.configService.keycloak_error);
          this.logger.error(
            'KeycloakService::getUsersInGroup Error has occured ',
            error
          );
          throw error;
        })
      );
  }
}

export interface UserRepresentation extends UserRepresentationBase {
  idir?: string;
  fullName?: string;
  jjDisplayName?: string;
}

export interface KeycloakProfile extends KeycloakProfileJS {
  idir?: string;
  fullName?: string;
  attributes?: any;
}
