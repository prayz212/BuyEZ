export class EnsureModuleLoadedOnceGuard<TModule extends object> {
  constructor(targetModule: TModule) {
    if (targetModule) {
      throw new Error(
        `${targetModule.constructor.name} has already been loaded. Import this module in the AppModule only.`
      );
    }
  }
}
