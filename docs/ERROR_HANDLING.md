# Error Handling v MyProject Aplikaci

Tento dokument popisuje, jak aplikace MyProject zpracovává chyby na různých úrovních architektury.

## Přehled architektury error handlingu

Aplikace používá vícevrstvý přístup k zpracování chyb:

1. **Server API Layer** (`/server/api/auth/[controller].ts`) - Zpracování chyb na úrovni server-side API
2. **Utility Layer** (`/util/api/error.ts`) - Centralizované utility funkce pro detekci a zpracování chyb
3. **Component Layer** (`/pages/auth/login.vue`) - Zpracování chyb na úrovni uživatelského rozhraní

## Server API Layer

### Hlavní funkce
Server API používá funkci `handleApiError()` pro standardizované zpracování chyb:

```typescript
import { handleApiError } from '~/util/api/error';

// Příklad použití v auth controlleru
try {
    const response = await $fetch(`${AUTH_ORIGIN}/api/Account/login`, {
        method: 'POST',
        body,
        credentials: 'include',
        headers: {
            'Content-Type': 'application/json',
        },
    });
    return response;
} catch (err: unknown) {
    handleApiError(err, t);
}
```

### Speciální zpracování chyb
- **401 chyby u logout/session**: Vrací se úspěšná odpověď (200/204), protože neautorizovaný logout/session je validní stav

## Utility Layer (`/util/api/error.ts`)

### Detekční funkce
Poskytuje sadu utility funkcí pro detekci specifických typů chyb:

```typescript
// Síťové chyby
isNetworkError(error) // Kombinuje fetch a timeout chyby
isFetchError(error)   // Chyby při fetch operacích
isTimeoutError(error) // Timeout chyby

// HTTP status chyby
isNotFoundError(error)        // 404
isBadRequestError(error)      // 400
isTooManyRequestsError(error) // 429
isServerError(error)          // 5xx
isNotAllowedError(error)      // 405
```

### Centralizované zpracování - `handleApiError()`
Funkce mapuje různé typy chyb na odpovídající HTTP response:

| Typ chyby | Status Code | Popis |
|-----------|-------------|-------|
| Method Not Allowed | 405 | Nepodporovaná HTTP metoda |
| Request Timeout | 408 | Timeout při požadavku |
| Connection Error | 503 | Chyba při připojení (fetch failed) |
| Not Found | 404 | Nenalezen resource |
| Bad Request | 400 | Chybný požadavek |
| Too Many Requests | 429 | Rate limiting |
| Internal Server Error | 500 | Obecná server chyba |
| Unexpected Error | 500 | Fallback pro neočekávané chyby |

### Bezpečné type checking
Používá type guards pro bezpečné ověření vlastností chyb:

```typescript
function isErrorWithProperty<T extends keyof ErrorObject>(
    error: unknown,
    property: T,
): error is ErrorObject & Record<T, NonNullable<ErrorObject[T]>> {
    return typeof error === 'object' && error !== null && property in error;
}
```

## Component Layer (Frontend)

### Lokální error handling v komponentách
Komponenty zpracovávají chyby lokálně s uživatelsky přívětivými zprávami:

```vue
<script setup lang="ts">
const error = ref('');

const onLogin = async () => {
    try {
        await signIn(credentials, { callbackUrl: callbackUrl || '/' });
    } catch (e: unknown) {
        switch (getErrorStatusCode(e)) {
            case 401:
                error.value = t('invalidCredentials');
                break;
            default:
                error.value = getErrorMessage(e) || t('loginError');
        }
    }
};
</script>
```

### Zobrazení chyb uživateli
```vue
<UAlert 
    v-if="error" 
    :title="error" 
    data-test-id="login-error" 
    color="error" 
    class="mt-2" 
/>
```

## Internacionalizace chyb

Všechny chybové zprávy jsou lokalizované pomocí `useI18n`:

```json
{
    "cs": {
        "invalidCredentials": "Neplatné přihlašovací údaje.",
        "loginError": "Chyba při přihlášení."
    },
    "en": {
        "invalidCredentials": "Invalid credentials.",
        "loginError": "Login error."
    }
}
```

## Best Practices

### 1. Konzistentní error handling
- Všechny API endpoints používají `handleApiError()` pro konzistentní zpracování
- Komponenty používají lokální error stavy s internacionalizací

### 2. Type Safety
- Všechny error handling funkce pracují s `unknown` typem
- Používají se type guards pro bezpečné ověření vlastností

### 3. User Experience
- Specifické chybové zprávy pro různé situace (401 → "Neplatné údaje")
- Loading stavy během operací
- Vizuální indikace chyb pomocí UI komponent

### 4. Debugging
- Server-side logování všech chyb

### 5. Graceful degradation
- Fallback zprávy pro neočekávané chyby
- Speciální zpracování pro běžné situace (neautorizovaný logout)

## Rozšíření error handlingu

Pro přidání nového typu chyby:

1. **Přidat detekční funkci** do `error.ts`:
```typescript
export function isMyCustomError(error: unknown): boolean {
    return getErrorStatusCode(error) === 422;
}
```

2. **Rozšířit `handleApiError()`**:
```typescript
if (isMyCustomError(error)) {
    throw createError({
        statusCode: 422,
        statusMessage: t('error.apiError.customError'),
    });
}
```

3. **Zpracovat v komponentě**:
```typescript
switch (getErrorStatusCode(e)) {
    case 422:
        error.value = t('customErrorMessage');
        break;
}
```