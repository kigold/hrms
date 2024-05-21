import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { AuthService } from "../services/auth.service";

export const authguardGuard: CanActivateFn = (route, state) => {
    const router = inject(Router);
    const authService = inject(AuthService)
    if (!authService.getUserProfile())
      router.navigate(['home']);
    return true;
  };